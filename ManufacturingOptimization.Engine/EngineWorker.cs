using Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
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
    private readonly IPipelineFactory _pipelineFactory;

    public EngineWorker(
        ILogger<EngineWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IProviderRepository providerRepository,
        IRecommendationEngine recommendationEngine,
        IPipelineFactory pipelineFactory)
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
            SubscribedQueues = new List<string> {
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
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.AllReady);
        _messagingInfrastructure.PurgeQueue("engine.provider.events");

        // Subscribe to provider events
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("engine.provider.events", HandleProviderRegistered);
        _messageSubscriber.Subscribe<AllProvidersReadyEvent>("engine.provider.events", async evt =>
        {
           await HandleOptimizationRequestAsync(new RequestOptimizationPlanCommand
           {
               CommandId = Guid.NewGuid()
           });
        });

        _messagingInfrastructure.DeclareQueue("engine.optimization.requests");
        _messagingInfrastructure.BindQueue("engine.optimization.requests", Exchanges.Optimization, "optimization.request");
        _messagingInfrastructure.PurgeQueue("engine.optimization.requests");

        // 1. Listen for New Requests (US-06)
        _messageSubscriber.Subscribe<RequestOptimizationPlanCommand>("engine.optimization.requests", async evt => await HandleOptimizationRequestAsync(evt));

        // 2. Listen for User Selection (US-07-T4)
        _messageSubscriber.Subscribe<SelectStrategyCommand>("optimization.strategy.selected", HandleStrategySelection);
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
        try
        {
            // Mock data for demonstration
            var motorRequest = new MotorRequest
            {
                RequestId = command.CommandId,
                Specs = new MotorSpecifications
                {
                    PowerKW = 5.5,
                    AxisHeightMM = 75,
                    CurrentEfficiency = EfficiencyClass.IE1,
                    TargetEfficiency = EfficiencyClass.IE4 // This will trigger Upgrade workflow (8 steps)
                },
                Constraints = new RequestConstraints
                {
                    MaxBudget = 10000,
                    Priority = OptimizationPriority.HighestQuality
                }
            };

            // Create workflow pipeline
            var pipeline = _pipelineFactory.CreateWorkflowPipeline();
            var context = new WorkflowContext { Request = motorRequest };

            // Execute pipeline
            await pipeline.ExecuteAsync(context);
            
            // Publish OptimizationPlanReadyEvent (with success or errors)
            PublishOptimizationPlan(command.CommandId, context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing optimization request");
        }
    }

    private void HandleStrategySelection(SelectStrategyCommand command)
    {
        _logger.LogInformation("✅ CUSTOMER SELECTION CONFIRMED!");
        _logger.LogInformation("   Request ID:  {RequestId}", command.RequestId);
        _logger.LogInformation("   Provider ID: {SelectedProviderId}", command.SelectedProviderId);
        _logger.LogInformation("   Strategy:    {SelectedStrategyName}", command.SelectedStrategyName);
    }

    private void PublishOptimizationPlan(Guid commandId, WorkflowContext context)
    {
        var plan = new OptimizationPlan
        {
            RequestId = context.Request.RequestId,
            WorkflowType = context.WorkflowType ?? "Unknown",
            IsSuccess = context.IsSuccess,
            Errors = context.Errors.ToList(),
            Steps = context.ProcessSteps
                .Where(s => s.SelectedProvider != null)
                .Select(s => new OptimizedProcessStep
                {
                    StepNumber = s.StepNumber,
                    Activity = s.Activity,
                    SelectedProviderId = s.SelectedProvider!.ProviderId,
                    SelectedProviderName = s.SelectedProvider!.ProviderName,
                    CostEstimate = s.SelectedProvider!.CostEstimate,
                    TimeEstimate = s.SelectedProvider!.TimeEstimate,
                    QualityScore = s.SelectedProvider!.QualityScore,
                    EmissionsKgCO2 = s.SelectedProvider!.EmissionsKgCO2
                }).ToList(),
            TotalCost = context.OptimizationResult?.TotalCost ?? 0,
            TotalDuration = context.OptimizationResult?.TotalDuration ?? TimeSpan.Zero,
            AverageQuality = context.OptimizationResult?.AverageQuality ?? 0,
            TotalEmissionsKgCO2 = context.OptimizationResult?.TotalEmissionsKgCO2 ?? 0,
            SolverStatus = context.OptimizationResult?.SolverStatus ?? "FAILED"
        };

        var planReadyEvent = new OptimizationPlanReadyEvent
        {
            CommandId = commandId,
            Plan = plan
        };

        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanReady, planReadyEvent);
    }
}
