using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Abstractions;

/// <summary>
/// A single step in the workflow optimization pipeline.
/// </summary>
public interface IWorkflowStep
{
    string Name { get; }
    
    Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default);
}
