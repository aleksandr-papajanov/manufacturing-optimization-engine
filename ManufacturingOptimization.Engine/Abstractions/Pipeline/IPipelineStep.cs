namespace ManufacturingOptimization.Engine.Abstractions.Pipeline;

/// <summary>
/// A single step in the workflow optimization pipeline.
/// </summary>
public interface IPipelineStep
{
    string Name { get; }
    
    Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default);
}
