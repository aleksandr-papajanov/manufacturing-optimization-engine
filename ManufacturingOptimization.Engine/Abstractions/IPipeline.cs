using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Abstractions;

/// <summary>
/// Represents a workflow optimization pipeline.
/// </summary>
public interface IPipeline
{
    Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default);
}
