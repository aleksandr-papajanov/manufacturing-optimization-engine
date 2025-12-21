using Docker.DotNet;
using Docker.DotNet.Models;
using ManufacturingOptimization.Common.Messaging;
using ManufacturingOptimization.ProviderRegistry.Abstractions;
using ManufacturingOptimization.ProviderRegistry.Entities;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.ProviderRegistry.Services;

/// <summary>
/// Production mode orchestrator - creates and manages provider containers via Docker API.
/// </summary>
public class DockerProviderOrchestrator : ProviderOrchestratorBase, IProviderOrchestrator
{
    private readonly IProviderRepository _repository;
    private readonly DockerSettings _dockerSettings;
    private readonly RabbitMqSettings _rabbitMqSettings;
    private readonly Dictionary<Guid, string> _runningProviders = []; // providerId -> containerId
    private string? _networkName;

    public DockerProviderOrchestrator(
        ILogger<DockerProviderOrchestrator> logger,
        IProviderRepository repository,
        IOptions<DockerSettings> dockerSettings,
        IOptions<RabbitMqSettings> rabbitMqSettings)
        : base(logger)
    {
        _repository = repository;
        _dockerSettings = dockerSettings.Value;
        _rabbitMqSettings = rabbitMqSettings.Value;
    }

    public async Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        var providers = await _repository.GetAllAsync(cancellationToken);
        
        foreach (var provider in providers.Where(p => p.Enabled))
        {
            try
            {
                await StartAsync(provider, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start provider {Type} ({Id})", provider.Type, provider.Id);
            }
        }
    }

    public async Task StartAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        if (_runningProviders.ContainsKey(provider.Id))
        {
            return;
        }

        var containerName = $"provider-{provider.Id}";
        var network = await GetNetworkAsync(cancellationToken);

        var createParams = new CreateContainerParameters
        {
            Name = containerName,
            Image = _dockerSettings.ProviderImage,
            Env = new List<string>
            {
                $"PROVIDER_TYPE={provider.Type}",
                $"{provider.Type}__ProviderId={provider.Id}",
                $"{provider.Type}__ProviderName={provider.Name}",
                $"RabbitMQ__Host={_rabbitMqSettings.Host}",
                $"RabbitMQ__Port={_rabbitMqSettings.Port}",
                $"RabbitMQ__Username={_rabbitMqSettings.Username}",
                $"RabbitMQ__Password={_rabbitMqSettings.Password}"
            },
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

            _runningProviders[provider.Id] = response.ID;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to start provider: {ex.Message}");
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