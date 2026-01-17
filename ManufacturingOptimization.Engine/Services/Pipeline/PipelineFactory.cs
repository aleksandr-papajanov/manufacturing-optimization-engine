using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Engine.Abstractions;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Factory for creating workflow processing pipelines with all required dependencies.
/// </summary>
public class PipelineFactory : IPipelineFactory
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly IProviderRepository _providerRepository;
    private readonly IMessagePublisher _messagePublisher;

    public PipelineFactory(
        ILoggerFactory loggerFactory,
        IProviderRepository providerRepository,
        IMessagePublisher messagePublisher)
    {
        _loggerFactory = loggerFactory;
        _providerRepository = providerRepository;
        _messagePublisher = messagePublisher;
    }

    public IPipeline CreateWorkflowPipeline()
    {
        var steps = new IPipelineStep[]
        {
            new WorkflowMatchingStep(_loggerFactory.CreateLogger<WorkflowMatchingStep>()),
            new ProviderMatchingStep(_providerRepository, _loggerFactory.CreateLogger<ProviderMatchingStep>()),
            new EstimationStep(_loggerFactory.CreateLogger<EstimationStep>(), _messagePublisher),
            new OptimizationStep(_loggerFactory.CreateLogger<OptimizationStep>())
        };

        return new WorkflowPipeline(steps, _loggerFactory.CreateLogger<WorkflowPipeline>());
    }
}
