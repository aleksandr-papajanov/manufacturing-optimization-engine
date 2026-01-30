using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.ProviderRegistry.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.ProviderRegistry.Services;

namespace ManufacturingOptimization.ProviderRegistry;

public class ProviderRegistryWorker : BackgroundService
{
    private readonly ILogger<ProviderRegistryWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessagePublisher _messagePublisher;

    private readonly IMessageSubscriber _subscriber;
    private readonly IServiceProvider _serviceProvider;

    private readonly IProviderOrchestrator _orchestrator;
    private readonly ISystemReadinessService _readinessService;

    public ProviderRegistryWorker(
        ILogger<ProviderRegistryWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessagePublisher messagePublisher,
        IMessageSubscriber subscriber,
        IServiceProvider serviceProvider,
        IProviderOrchestrator orchestrator,
        ISystemReadinessService readinessService)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messagePublisher = messagePublisher;
        _subscriber = subscriber;
        _serviceProvider = serviceProvider;
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

        await SubscribeToMessages();

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
        //_messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        //_messagingInfrastructure.DeclareExchange(Exchanges.System);
    }

    private async Task SubscribeToMessages()
    {
        // 1. Subscribe to Phase 1: Quote Requests
        await _subscriber.SubscribeAsync<ProviderQuoteRequest>(
            Exchanges.Provider,
            "provider.quote.request",
            async (msg) => 
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<QuoteRequestHandler>();
                await handler.HandleAsync(msg);
            },
            "provider-quote-queue");

        // 2. Subscribe to Phase 2: Execute Commands
        await _subscriber.SubscribeAsync<ExecuteProcessCommand>(
            Exchanges.Process,
            "process.execute.#", // Listen for any command routed to process.execute.*
            async (msg) => 
            {
                using var scope = _serviceProvider.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<ExecuteProcessCommandHandler>();
                await handler.HandleAsync(msg);
            },
            "provider-execution-queue");
    }
}
