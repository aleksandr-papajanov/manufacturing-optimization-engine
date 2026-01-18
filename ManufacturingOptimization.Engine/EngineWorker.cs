using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.PanManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;
using ManufacturingOptimization.Engine.Services.Pipeline;

namespace ManufacturingOptimization.Engine;

public class EngineWorker : BackgroundService
{
    private readonly ILogger<EngineWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProviderRepository _providerRepository;
    private readonly IRecommendationEngine _recommendationEngine;
    private readonly IWorkflowPipelineFactory _pipelineFactory;

    public EngineWorker(
        ILogger<EngineWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IProviderRepository providerRepository,
        IRecommendationEngine recommendationEngine,
        IWorkflowPipelineFactory pipelineFactory)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _providerRepository = providerRepository;
        _recommendationEngine = recommendationEngine;
        _pipelineFactory = pipelineFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        SetupRabbitMq();

        // Give services time to start and subscribe
        await Task.Delay(1000, cancellationToken);

        // Engine is ready
        var evt = new ServiceReadyEvent
        {
            ServiceName = "Engine",
            SubscribedQueues = {
                "engine.provider.events",
                "engine.optimization.requests",
                "engine.provider.validation"
            }
        };

        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, evt);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    private void SetupRabbitMq()
    {
        // System coordination
        _messagingInfrastructure.DeclareExchange(Exchanges.System);
        _messagingInfrastructure.DeclareQueue("engine.system.ready");
        _messagingInfrastructure.BindQueue("engine.system.ready", Exchanges.System, SystemRoutingKeys.SystemReady);
        _messagingInfrastructure.PurgeQueue("engine.system.ready");
        _messageSubscriber.Subscribe<SystemReadyEvent>("engine.system.ready", HandleSystemReady);
        
        // Provider events
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        _messagingInfrastructure.DeclareQueue("engine.provider.events");
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.AllRegistered);
        _messagingInfrastructure.PurgeQueue("engine.provider.events");
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("engine.provider.events", HandleProviderRegistered);

        // Optimization requests
        _messagingInfrastructure.DeclareQueue("engine.optimization.requests");
        _messagingInfrastructure.BindQueue("engine.optimization.requests", Exchanges.Optimization, OptimizationRoutingKeys.PlanRequested);
        _messagingInfrastructure.PurgeQueue("engine.optimization.requests");
        _messageSubscriber.Subscribe<RequestOptimizationPlanCommand>("engine.optimization.requests", async evt => await HandleOptimizationRequestAsync(evt));
    }
    
    private void HandleSystemReady(SystemReadyEvent evt)
    {
        
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

    private async Task HandleOptimizationRequestAsync(RequestOptimizationPlanCommand command)
    {
        var pipeline = _pipelineFactory.CreateWorkflowPipeline();
        var context = new WorkflowContext { Request = command.Request };

        await pipeline.ExecuteAsync(context);

        if (!context.IsSuccess)
        {
            _logger.LogWarning("Optimization workflow failed for request {RequestId} with errors: {Errors}", context.Request.RequestId, string.Join("; ", context.Errors));
        }
    }
}