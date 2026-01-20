using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.ProviderRegistry.Abstractions;
using System.Threading.Tasks;

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
    private readonly HashSet<Guid> _registeredProviders = [];
    
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
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("compose.orchestrator.provider.registered", async evt => await HandleProviderRegistered(evt));
    }
    
    private async Task HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        bool allRegistered;

        lock (_registeredProviders)
        {
            _registeredProviders.Add(evt.Provider.Id);
            allRegistered = _registeredProviders.Count == EXPECTED_PROVIDERS;
        }

        if (allRegistered)
        {
            await Task.Delay(2000); // Small delay to ensure all processing is complete
            _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.AllRegistered, new AllProvidersRegisteredEvent());
        }
    }

    public Task StartAllAsync(CancellationToken cancellationToken = default)
    {
        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.RequestRegistrationAll, new RequestProvidersRegistrationCommand());
        return Task.CompletedTask;
    }

    public Task StartAsync(Provider provider, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAsync(Guid providerId, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task StopAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}