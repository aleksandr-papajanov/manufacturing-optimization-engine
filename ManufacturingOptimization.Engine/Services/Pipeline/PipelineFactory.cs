using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Engine.Abstractions;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Factory for creating workflow processing pipelines with all required dependencies.
/// </summary>
public class PipelineFactory : IWorkflowPipelineFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IProviderRepository _providerRepository;
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagingInfrastructure _messagingInfrastructure;

    public PipelineFactory(
        ILoggerFactory loggerFactory,
        IProviderRepository providerRepository,
        IOptimizationPlanRepository planRepository,
        IMessagePublisher messagePublisher,
        IMessageSubscriber messageSubscriber,
        IMessagingInfrastructure messagingInfrastructure)
    {
        _loggerFactory = loggerFactory;
        _providerRepository = providerRepository;
        _planRepository = planRepository;
        _messagePublisher = messagePublisher;
        _messageSubscriber = messageSubscriber;
        _messagingInfrastructure = messagingInfrastructure;
    }

    public IWorkflowPipeline CreateWorkflowPipeline()
    {
        var steps = new IWorkflowStep[]
        {
            new WorkflowMatchingStep(),
            new ProviderMatchingStep(_providerRepository),
            new EstimationStep(_messagePublisher),
            new OptimizationStep(),
            new StrategySelectionStep(_messagePublisher, _messagingInfrastructure, _messageSubscriber),
            new ConfirmationStep(_messagePublisher),
            new PlanPersistenceStep(_planRepository, _messagePublisher)
        };

        return new WorkflowPipeline(steps, _loggerFactory.CreateLogger<WorkflowPipeline>());
    }
}

