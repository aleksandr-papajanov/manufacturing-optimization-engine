using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Gateway.Services;

public class GatewayWorker : BackgroundService
{
    private readonly ILogger<GatewayWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IServiceProvider _serviceProvider;
    private readonly ISystemReadinessService _readinessService;

    public GatewayWorker(
        ILogger<GatewayWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IServiceProvider serviceProvider,
        ISystemReadinessService readinessService)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _serviceProvider = serviceProvider;
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

    private async void HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        using var scope = _serviceProvider.CreateScope();
        var providerRepo = scope.ServiceProvider.GetRequiredService<IProviderRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        
        var entity = mapper.Map<ProviderEntity>(evt.Provider);
        await providerRepo.AddAsync(entity);
        await providerRepo.SaveChangesAsync();
    }

    private async void HandleOptimizationResponse(OptimizationPlanReadyEvent evt)
    {
        using var scope = _serviceProvider.CreateScope();
        var planRepo = scope.ServiceProvider.GetRequiredService<IOptimizationPlanRepository>();
        var strategyRepo = scope.ServiceProvider.GetRequiredService<IOptimizationStrategyRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        
        // Map and save the plan
        var planEntity = mapper.Map<OptimizationPlanEntity>(evt.Plan);
        await planRepo.AddAsync(planEntity);
        await planRepo.SaveChangesAsync();
        
        // Get strategies for this request (not yet assigned to a plan)
        var unusedStrategies = await strategyRepo.GetForRequesttAsync(evt.Plan.RequestId);
        
        if (unusedStrategies != null)
        {
            foreach (var strategy in unusedStrategies)
            {
                // Update the selected strategy with PlanId
                if (strategy.Id == evt.Plan.SelectedStrategy.Id)
                {
                    strategy.PlanId = planEntity.Id;
                }
            }
            
            // Save changes to assign PlanId
            await strategyRepo.SaveChangesAsync();
        }
        
        // Remove unused strategies for this request (those without PlanId)
        await strategyRepo.RemoveForRequestAsync(evt.Plan.RequestId);
    }

    private void HandleStrategiesReady(MultipleStrategiesReadyEvent evt)
    {
        using var scope = _serviceProvider.CreateScope();
        var strategyRepo = scope.ServiceProvider.GetRequiredService<IOptimizationStrategyRepository>();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        
        // Map strategies and set RequestId for filtering
        var entities = mapper.Map<List<OptimizationStrategyEntity>>(evt.Strategies);
        
        foreach (var entity in entities)
        {
            entity.RequestId = evt.RequestId;
            entity.PlanId = null; // No plan yet
        }
        
        strategyRepo.AddForRequestAsync(evt.RequestId, entities);
    }
}