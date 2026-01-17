using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.ProviderRegistry.Abstractions;
using ManufacturingOptimization.ProviderRegistry.Services;

namespace ManufacturingOptimization.ProviderRegistry;

public class ProviderRegistryWorker : BackgroundService
{
    private readonly ILogger<ProviderRegistryWorker> _logger;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProviderOrchestrator _orchestrator;

    public ProviderRegistryWorker(
        ILogger<ProviderRegistryWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IProviderRepository providerRepository,
        IProviderOrchestrator orchestrator)
    {
        _logger = logger;
        _providerRepository = providerRepository;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _orchestrator = orchestrator;

    }

    // Ovwerride StartAsync to perform cleanup before starting
    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _orchestrator.CleanupOrphanedContainersAsync(cancellationToken);
        await base.StartAsync(cancellationToken);
    }

    // Override StopAsync to stop all providers gracefully
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _orchestrator.StopAllAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        SetupRabbitMq();

        // Give subscriptions time to register
        await Task.Delay(1000, cancellationToken);
        
        // Publish service ready event
        var readyEvent = new ServiceReadyEvent
        {
            ServiceName = "ProviderRegistry",
            SubscribedQueues = new List<string> { "registry.provider.registered", "registry.validation.commands", "dev.tracker.provider.registered", "registry.system.ready" }
        };

        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, readyEvent);
        
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private void SetupRabbitMq()
    {
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        _messagingInfrastructure.DeclareExchange(Exchanges.System);
        
        // Subscribe to SystemReadyEvent to start providers
        _messagingInfrastructure.DeclareQueue("registry.system.ready");
        _messagingInfrastructure.BindQueue("registry.system.ready", Exchanges.System, SystemRoutingKeys.SystemReady);
        _messagingInfrastructure.PurgeQueue("registry.system.ready");
        
        _messageSubscriber.Subscribe<SystemReadyEvent>("registry.system.ready", HandleSystemReady);
    }
    
    private async void HandleSystemReady(SystemReadyEvent evt)
    {
        await _orchestrator.StartAllAsync();
    }
}
