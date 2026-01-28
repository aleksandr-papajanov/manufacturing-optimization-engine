using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;

namespace ManufacturingOptimization.Engine;

public class EngineWorker : BackgroundService
{
    private readonly ILogger<EngineWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessageDispatcher _dispatcher;

    public EngineWorker(
        ILogger<EngineWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IMessageDispatcher dispatcher)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _dispatcher = dispatcher;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        SetupRabbitMq();

        // Give services time to start and subscribe
        await Task.Delay(1000, cancellationToken);

        // Engine is ready
        var evt = new ServiceReadyEvent
        {
            ServiceName = "Engine"
        };

        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, evt);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private void SetupRabbitMq()
    {
        // Provider events
        _messagingInfrastructure.DeclareQueue("engine.provider.events");
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.AllRegistered);
        _messagingInfrastructure.PurgeQueue("engine.provider.events");
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("engine.provider.events", e => _dispatcher.DispatchAsync(e));

        // Optimization requests
        _messagingInfrastructure.DeclareQueue("engine.optimization.requests");
        _messagingInfrastructure.BindQueue("engine.optimization.requests", Exchanges.Optimization, OptimizationRoutingKeys.PlanRequested);
        _messagingInfrastructure.PurgeQueue("engine.optimization.requests");
        _messageSubscriber.Subscribe<RequestOptimizationPlanCommand>("engine.optimization.requests", e => _dispatcher.DispatchAsync(e));
    }
}