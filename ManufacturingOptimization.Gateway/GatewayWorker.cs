using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Gateway.Abstractions;

namespace ManufacturingOptimization.Gateway.Services;

public class GatewayWorker : BackgroundService
{
    private readonly ILogger<GatewayWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProviderRepository _providerRepository;
    private readonly IOptimizationStrategyRepository _strategyRepository;
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly ISystemReadinessService _readinessService;

    public GatewayWorker(
        ILogger<GatewayWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IProviderRepository providerRepository,
        IOptimizationStrategyRepository strategyRepository,
        IOptimizationPlanRepository planRepository,
        ISystemReadinessService readinessService)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;;
        _providerRepository = providerRepository;
        _strategyRepository = strategyRepository;
        _planRepository = planRepository;
        _readinessService = readinessService;
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

        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("gateway.provider.events", HandleProviderRegistered);
        _messageSubscriber.Subscribe<OptimizationPlanReadyEvent>("gateway.optimization.responses", HandleOptimizationResponse);
        _messageSubscriber.Subscribe<MultipleStrategiesReadyEvent>("gateway.strategies.ready", HandleStrategiesReady);
    }

    private void HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        _providerRepository.Create(evt.Provider);
    }

    private void HandleOptimizationResponse(OptimizationPlanReadyEvent evt)
    {
        _planRepository.Create(evt.Plan);
    }

    private void HandleStrategiesReady(MultipleStrategiesReadyEvent evt)
    {
        _strategyRepository.StoreStrategies(evt.RequestId, evt.Strategies);
    }
}