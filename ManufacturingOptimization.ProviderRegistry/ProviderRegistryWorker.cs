using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.ProviderRegistry.Abstractions;

namespace ManufacturingOptimization.ProviderRegistry;

public class ProviderRegistryWorker : BackgroundService
{
    private readonly ILogger<ProviderRegistryWorker> _logger;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProviderOrchestrator _orchestrator;
    private readonly ISystemReadinessService _readinessService;

    public ProviderRegistryWorker(
        ILogger<ProviderRegistryWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IProviderRepository providerRepository,
        IProviderOrchestrator orchestrator,
        ISystemReadinessService readinessService)
    {
        _logger = logger;
        _providerRepository = providerRepository;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _orchestrator = orchestrator;
        _readinessService = readinessService;

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
            ServiceName = "ProviderRegistry"
        };

        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, readyEvent);
        
        // Wait for system to be ready, then start providers
        await _readinessService.WaitForSystemReadyAsync(cancellationToken);
        await _orchestrator.StartAllAsync(cancellationToken);
        
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private void SetupRabbitMq()
    {
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        _messagingInfrastructure.DeclareExchange(Exchanges.System);
    }
}
