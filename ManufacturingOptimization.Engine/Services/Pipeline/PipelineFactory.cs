using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Data.Repositories;
using AutoMapper;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Factory for creating workflow processing pipelines with all required dependencies.
/// </summary>
public class PipelineFactory : IWorkflowPipelineFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IProviderRepository _providerRepository;
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly IOptimizationStrategyRepository _strategyRepository;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMapper _mapper;

    public PipelineFactory(
        ILoggerFactory loggerFactory,
        IProviderRepository providerRepository,
        IOptimizationPlanRepository planRepository,
        IOptimizationStrategyRepository strategyRepository,
        IMessagePublisher messagePublisher,
        IMessageSubscriber messageSubscriber,
        IMessagingInfrastructure messagingInfrastructure,
        IMapper mapper)
    {
        _loggerFactory = loggerFactory;
        _providerRepository = providerRepository;
        _planRepository = planRepository;
        _strategyRepository = strategyRepository;
        _messagePublisher = messagePublisher;
        _messageSubscriber = messageSubscriber;
        _messagingInfrastructure = messagingInfrastructure;
        _mapper = mapper;
    }

    public IWorkflowPipeline CreateWorkflowPipeline()
    {
        var steps = new IWorkflowStep[]
        {
            new WorkflowMatchingStep(),
            new ProviderMatchingStep(_providerRepository),
            new EstimationStep(_messagePublisher),
            new OptimizationStep(),
            new StrategySelectionStep(_messagePublisher, _messagingInfrastructure, _messageSubscriber, _mapper),
            new ConfirmationStep(_messagePublisher),
            new PlanPersistenceStep(_mapper, _planRepository, _strategyRepository, _messagePublisher)
        };

        return new WorkflowPipeline(steps, _loggerFactory.CreateLogger<WorkflowPipeline>());
    }
}

