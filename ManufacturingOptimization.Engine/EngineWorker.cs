using Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Abstractions.Pipeline;
using ManufacturingOptimization.Engine.Services.Pipeline;

namespace ManufacturingOptimization.Engine;

public class EngineWorker : BackgroundService
{
    private readonly ILogger<EngineWorker> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProviderRepository _providerRepository;
    private readonly IRecommendationEngine _recommendationEngine;

    public EngineWorker(
        ILogger<EngineWorker> logger,
        ILoggerFactory loggerFactory,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IProviderRepository providerRepository,
        IRecommendationEngine recommendationEngine)
    {
        _logger = logger;
        _loggerFactory = loggerFactory;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _providerRepository = providerRepository;
        _recommendationEngine = recommendationEngine;

        SetupRabbitMq();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Engine Worker started. Waiting for requests...");

        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.StartAll, new StartAllProvidersCommand());

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        // Provider events
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        _messagingInfrastructure.DeclareQueue("engine.provider.events");
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.AllReady);
        
        // Subscribe to provider events
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("engine.provider.events", HandleProviderRegistered);
        
        _messagingInfrastructure.DeclareQueue("engine.optimization.requests");
        _messagingInfrastructure.BindQueue("engine.optimization.requests", Exchanges.Optimization, "optimization.request");
        
        // 1. Listen for New Requests (US-06)
        _messageSubscriber.Subscribe<RequestOptimizationPlanCommand>("engine.optimization.requests", HandleOptimizationRequest);

        // 2. Listen for User Selection (US-07-T4)
        _messageSubscriber.Subscribe<SelectStrategyCommand>("optimization.strategy.selected", HandleStrategySelection);
    }

    private void HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        _logger.LogInformation("Provider registered: {ProviderId} ({ProviderName}) with {CapabilityCount} capabilities",
            evt.ProviderId, evt.ProviderName, evt.Capabilities.Count);
        
        _providerRepository.Create(
            evt.ProviderId, 
            evt.ProviderType, 
            evt.ProviderName, 
            evt.Capabilities,
            evt.TechnicalCapabilities.AxisHeight,
            evt.TechnicalCapabilities.Power,
            evt.TechnicalCapabilities.Tolerance);
    }

    private async void HandleOptimizationRequest(RequestOptimizationPlanCommand command)
    {
        _logger.LogInformation("Processing request {RequestId}", command.CommandId);
        
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
            var pipeline = CreateWorkflowPipeline();
            var context = new WorkflowContext { Request = motorRequest };

            // Execute pipeline
            await pipeline.ExecuteAsync(context);

            if (context.IsSuccess)
            {
                _logger.LogInformation("Workflow: {WorkflowType} ({StepCount} steps)", 
                    context.WorkflowType, context.ProcessSteps.Count);
                
                foreach (var step in context.ProcessSteps)
                {
                    _logger.LogInformation("  Step {StepNumber}: {Activity} - {ProviderCount} providers matched",
                        step.StepNumber, step.Activity, step.MatchedProviders.Count);
                }
            }
            else
            {
                _logger.LogError("Pipeline failed: {Errors}", string.Join(", ", context.Errors));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing optimization request");
        }
    }

    private IPipeline CreateWorkflowPipeline()
    {
        var steps = new IPipelineStep[]
        {
            new WorkflowMatchingStep(_loggerFactory.CreateLogger<WorkflowMatchingStep>()),
            new ProviderMatchingStep(_providerRepository, _loggerFactory.CreateLogger<ProviderMatchingStep>())
        };

        return new WorkflowPipeline(steps, _loggerFactory.CreateLogger<WorkflowPipeline>());
    }

    private void HandleStrategySelection(SelectStrategyCommand command)
    {
        _logger.LogInformation("✅ CUSTOMER SELECTION CONFIRMED!");
        _logger.LogInformation("   Request ID:  {RequestId}", command.RequestId);
        _logger.LogInformation("   Provider ID: {SelectedProviderId}", command.SelectedProviderId);
        _logger.LogInformation("   Strategy:    {SelectedStrategyName}", command.SelectedStrategyName);
    }
}
