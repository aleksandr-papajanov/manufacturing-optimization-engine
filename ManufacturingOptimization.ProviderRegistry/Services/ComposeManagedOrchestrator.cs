using Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.ProviderRegistry.Abstractions;

namespace ManufacturingOptimization.ProviderRegistry.Services;

/// <summary>
/// Development mode orchestrator - providers managed by docker-compose.
/// Tracks ProviderRegisteredEvent and publishes AllProvidersReadyEvent when all 3 providers register.
/// </summary>
public class ComposeManagedOrchestrator : ProviderOrchestratorBase, IProviderOrchestrator
{
    private const int EXPECTED_PROVIDERS = 3;
    
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly HashSet<Guid> _registeredProviders = new();
    
    public ComposeManagedOrchestrator(
        ILogger<ComposeManagedOrchestrator> logger,
        IMessagePublisher messagePublisher,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber)
        : base(logger)
    {
        _messagePublisher = messagePublisher;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        
        SetupRabbitMq();
    }
    
    private void SetupRabbitMq()
    {
        _messagingInfrastructure.DeclareQueue("compose.orchestrator.provider.registered");
        _messagingInfrastructure.BindQueue("compose.orchestrator.provider.registered", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.PurgeQueue("compose.orchestrator.provider.registered");
        
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("compose.orchestrator.provider.registered", HandleProviderRegistered);
    }
    
    private void HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        lock (_registeredProviders)
        {
            // Track this provider as registered
            if (_registeredProviders.Add(evt.ProviderId))
            {
                // Check if all expected providers have registered
                if (_registeredProviders.Count == EXPECTED_PROVIDERS)
                {
                    _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.AllReady, new AllProvidersReadyEvent());
                }
            }
        }
    }

    public Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Development mode: sending StartAllProvidersCommand to provider simulators");
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.StartAll, new StartAllProvidersCommand());
        return Task.CompletedTask;
    }

    public Task StartAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Development mode: provider {Type} managed by docker-compose", provider.Type);
        return Task.CompletedTask;
    }

    public Task StopAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Development mode: providers managed by docker-compose");
        return Task.CompletedTask;
    }

    public Task StopAllAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Development mode: providers managed by docker-compose");
        return Task.CompletedTask;
    }
}