using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Abstractions;

/// <summary>
/// Represents a workflow optimization pipeline.
/// </summary>
public interface IWorkflowPipeline
{
    Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default);
}
