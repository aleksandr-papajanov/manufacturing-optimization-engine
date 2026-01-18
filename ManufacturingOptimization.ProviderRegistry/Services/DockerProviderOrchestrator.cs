using Common.Models;
using Docker.DotNet.Models;
using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.ProviderRegistry.Abstractions;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.ProviderRegistry.Services;

/// <summary>
/// Production mode orchestrator - creates and manages provider containers via Docker API.
/// </summary>
public class DockerProviderOrchestrator : ProviderOrchestratorBase, IProviderOrchestrator
{
    private readonly IProviderRepository _repository;
    private readonly IProviderValidationService _validationService;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly DockerSettings _dockerSettings;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly Dictionary<Guid, string> _runningProviders = []; // providerId -> containerId
    private readonly HashSet<Guid> _registeredProviders = new(); // Track ProviderRegisteredEvents
    private string? _networkName;

    public DockerProviderOrchestrator(
        ILogger<DockerProviderOrchestrator> logger,
        IProviderRepository repository,
        IProviderValidationService validationService,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IOptions<DockerSettings> dockerSettings,
        IOptions<RabbitMqSettings> rabbitMqSettings)
        : base(logger)
    {
        _repository = repository;
        _validationService = validationService;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _dockerSettings = dockerSettings.Value;
        _rabbitMqSettings = rabbitMqSettings.Value;
        
        SetupRabbitMq();
    }
    
    private void SetupRabbitMq()
    {
        _messagingInfrastructure.DeclareQueue("orchestrator.provider.registered");
        _messagingInfrastructure.BindQueue("orchestrator.provider.registered", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.PurgeQueue("orchestrator.provider.registered");
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("orchestrator.provider.registered", async evt => await HandleProviderRegistered(evt));
    }
    
    private async Task HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        bool allRegistered;
        int runningCount;
        int registeredCount;
        
        lock (_registeredProviders)
        {
            _registeredProviders.Add(evt.ProviderId);
            registeredCount = _registeredProviders.Count;
            
            lock (_runningProviders)
            {
                runningCount = _runningProviders.Count;
                allRegistered = _registeredProviders.IsSupersetOf(_runningProviders.Keys);
            }
        }

        if (allRegistered)
        {
            // Wait a moment to ensure all registrations are processed
            await Task.Delay(2000);
            _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.AllRegistered, new AllProvidersRegisteredEvent());
        }
    }

    public async Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        var providers = await _repository.GetAllAsync(cancellationToken);
        var enabledProviders = providers.Where(p => p.Enabled).ToList();
        
        if (!enabledProviders.Any())
        {
            return;
        }
        
        foreach (var provider in enabledProviders)
        {
            try
            {
                var (isApproved, declinedReason) = await _validationService.ValidateAsync(provider, cancellationToken: cancellationToken);
                
                if (!isApproved)
                {
                    _logger.LogWarning("Provider {Type} ({Id}) declined during validation: {Reason}", provider.Type, provider.Id, declinedReason);
                    continue;
                }
                
                await StartAsync(provider, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to validate/start provider {Type} ({Id})", provider.Type, provider.Id);
            }
        }

        // Important: wait a moment before requesting registrations to ensure all containers are ready
        await Task.Delay(3000, cancellationToken);
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.RequestRegistrationAll, new RequestProvidersRegistrationCommand());
    }

    public async Task StartAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        if (_runningProviders.ContainsKey(provider.Id))
        {
            return;
        }

        var containerName = $"provider-{provider.Id}";
        var network = await GetNetworkAsync(cancellationToken);

        // Build environment variables
        var envVars = new List<string>
        {
            $"PROVIDER_TYPE={provider.Type}",
            $"{provider.Type}__ProviderId={provider.Id}",
            $"{provider.Type}__ProviderName={provider.Name}",
            $"RabbitMQ__Host={_rabbitMqSettings.Host}",
            $"RabbitMQ__Port={_rabbitMqSettings.Port}",
            $"RabbitMQ__Username={_rabbitMqSettings.Username}",
            $"RabbitMQ__Password={_rabbitMqSettings.Password}"
        };

        // Add process capabilities
        for (int i = 0; i < provider.ProcessCapabilities.Count; i++)
        {
            var capability = provider.ProcessCapabilities[i];
            envVars.Add($"{provider.Type}__ProcessCapabilities__{i}__ProcessName={capability.ProcessName}");
            envVars.Add($"{provider.Type}__ProcessCapabilities__{i}__CostPerHour={capability.CostPerHour}");
            envVars.Add($"{provider.Type}__ProcessCapabilities__{i}__QualityScore={capability.QualityScore}");
            envVars.Add($"{provider.Type}__ProcessCapabilities__{i}__CarbonIntensityKgCO2PerKwh={capability.CarbonIntensityKgCO2PerKwh}");
        }

        // Add technical capabilities
        envVars.Add($"{provider.Type}__AxisHeight={provider.TechnicalCapabilities.AxisHeight}");
        envVars.Add($"{provider.Type}__Power={provider.TechnicalCapabilities.Power}");
        envVars.Add($"{provider.Type}__Tolerance={provider.TechnicalCapabilities.Tolerance}");

        var createParams = new CreateContainerParameters
        {
            Name = containerName,
            Image = _dockerSettings.ProviderImage,
            Env = envVars,
            HostConfig = new HostConfig
            {
                NetworkMode = network,
                AutoRemove = true
            },
            Labels = new Dictionary<string, string>
            {
                ["orchestration-mode"] = "production"
            }
        };

        try
        {
            var response = await _dockerClient.Containers.CreateContainerAsync(createParams, cancellationToken);
            await _dockerClient.Containers.StartContainerAsync(response.ID, new ContainerStartParameters(), cancellationToken);

            lock (_runningProviders)
            {
                _runningProviders[provider.Id] = response.ID;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start provider container: {Message}", ex.Message);
            throw;
        }
    }

    public async Task StopAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        if (!_runningProviders.TryGetValue(providerId, out var containerId))
        {
            return;
        }

        try
        {
            await _dockerClient.Containers.StopContainerAsync(
                containerId,
                new ContainerStopParameters { WaitBeforeKillSeconds = 30 },
                cancellationToken);

            _runningProviders.Remove(providerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to stop provider {providerId}");
        }
    }

    public async Task StopAllAsync(CancellationToken cancellationToken = default)
    {
        var providerIds = _runningProviders.Keys.ToList();
        foreach (var providerId in providerIds)
        {
            await StopAsync(providerId, cancellationToken);
        }
    }

    private async Task<string> GetNetworkAsync(CancellationToken cancellationToken)
    {
        if (_dockerSettings.Network != "auto")
            return _dockerSettings.Network;

        if (_networkName != null)
            return _networkName;

        try
        {
            var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
            if (string.IsNullOrEmpty(hostname))
            {
                _networkName = "bridge";
                return _networkName;
            }

            var container = await _dockerClient.Containers.InspectContainerAsync(hostname, cancellationToken);
            _networkName = container.NetworkSettings.Networks.Keys.FirstOrDefault() ?? "bridge";
            
            return _networkName;
        }
        catch
        {
            _networkName = "bridge";
            return _networkName;
        }
    }
}