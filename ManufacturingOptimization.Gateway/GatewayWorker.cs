using Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.Gateway.Abstractions;

namespace ManufacturingOptimization.Gateway.Services;

public class GatewayWorker : BackgroundService
{
    private readonly ILogger<GatewayWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IRequestResponseRepository _repository;
    private readonly IProviderRepository _providerRepository;
    private readonly StrategyCacheService _strategyCache;

    public GatewayWorker(
        ILogger<GatewayWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IRequestResponseRepository repository,
        IProviderRepository providerRegistry,
        StrategyCacheService strategyCache)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _repository = repository;
        _providerRepository = providerRegistry;
        _strategyCache = strategyCache;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetupRabbitMq();
        
        // Give subscriptions time to register
        await Task.Delay(1000, stoppingToken);
        
        // Publish service ready event
        var readyEvent = new ServiceReadyEvent
        {
            ServiceName = "Gateway",
            SubscribedQueues = new List<string> 
            { 
                "gateway.provider.events", 
                "gateway.optimization.responses",
                "gateway.strategies.ready"
            }
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
        _messagingInfrastructure.BindQueue("gateway.optimization.responses", Exchanges.Optimization, "optimization.response");
        _messagingInfrastructure.PurgeQueue("gateway.optimization.responses");

        // Listen to strategies ready events (US-07)
        _messagingInfrastructure.DeclareQueue("gateway.strategies.ready");
        _messagingInfrastructure.BindQueue("gateway.strategies.ready", Exchanges.Optimization, OptimizationRoutingKeys.StrategiesReady);
        _messagingInfrastructure.PurgeQueue("gateway.strategies.ready");

        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("gateway.provider.events", HandleProviderRegistered);
        _messageSubscriber.Subscribe<OptimizationPlanCreatedEvent>("gateway.optimization.responses", HandleOptimizationResponse);
        _messageSubscriber.Subscribe<MultipleStrategiesReadyEvent>("gateway.strategies.ready", HandleStrategiesReady);
    }

    private void HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        var provider = new Provider
        {
            Id = evt.ProviderId,
            Type = evt.ProviderType,
            Name = evt.ProviderName,
            Enabled = true,
            ProcessCapabilities = evt.ProcessCapabilities,
            TechnicalCapabilities = evt.TechnicalCapabilities
        };
        
        _providerRepository.Create(provider);
    }

    private void HandleOptimizationResponse(OptimizationPlanCreatedEvent response)
    {
        _repository.AddResponse(response);
    }

    private void HandleStrategiesReady(MultipleStrategiesReadyEvent evt)
    {
        _logger.LogInformation(
            "Received {Count} strategies for Request {RequestId}. Caching for customer retrieval.",
            evt.Strategies.Count,
            evt.RequestId);

        // Store strategies in cache for HTTP polling
        _strategyCache.StoreStrategies(evt.RequestId, evt.Strategies);

        _logger.LogInformation(
            "Successfully cached strategies for Request {RequestId}. Customer can now retrieve via HTTP API.",
            evt.RequestId);
    }
}