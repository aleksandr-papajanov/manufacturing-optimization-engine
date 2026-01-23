using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;

namespace ManufacturingOptimization.Gateway.Services;

public class GatewayWorker : BackgroundService
{
    private readonly ILogger<GatewayWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessageDispatcher _dispatcher;

    public GatewayWorker(
        ILogger<GatewayWorker> logger,
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

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetupRabbitMq();
        
        // Give subscriptions time to register
        await Task.Delay(1000, stoppingToken);
        
        // Publish service ready event
        var readyEvent = new ServiceReadyEvent
        {
            ServiceName = "Gateway"
        };

        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, readyEvent);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        // Listen to provider registrations
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        _messagingInfrastructure.DeclareQueue("gateway.provider.events");
        _messagingInfrastructure.BindQueue("gateway.provider.events", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.PurgeQueue("gateway.provider.events");

        // Listen to optimization responses
        _messagingInfrastructure.DeclareExchange(Exchanges.Optimization);
        _messagingInfrastructure.DeclareQueue("gateway.optimization.responses");
        _messagingInfrastructure.BindQueue("gateway.optimization.responses", Exchanges.Optimization, OptimizationRoutingKeys.PlanReady);
        _messagingInfrastructure.PurgeQueue("gateway.optimization.responses");

        // Listen to strategies ready events (US-07)
        _messagingInfrastructure.DeclareQueue("gateway.strategies.ready");
        _messagingInfrastructure.BindQueue("gateway.strategies.ready", Exchanges.Optimization, OptimizationRoutingKeys.StrategiesReady);
        _messagingInfrastructure.PurgeQueue("gateway.strategies.ready");

        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("gateway.provider.events", e => _dispatcher.DispatchAsync(e));
        _messageSubscriber.Subscribe<OptimizationPlanReadyEvent>("gateway.optimization.responses", e => _dispatcher.DispatchAsync(e));
        _messageSubscriber.Subscribe<MultipleStrategiesReadyEvent>("gateway.strategies.ready", e => _dispatcher.DispatchAsync(e));
    }
}