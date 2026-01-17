namespace ManufacturingOptimization.Engine.Abstractions.Pipeline;

/// <summary>
/// Represents a workflow optimization pipeline.
/// </summary>
public interface IPipeline
{
    Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default);
}
