namespace ManufacturingOptimization.Engine.Abstractions;

/// <summary>
/// Factory for creating workflow processing pipelines.
/// </summary>
public interface IWorkflowPipelineFactory
{
    /// <summary>
    /// Creates a new workflow pipeline instance.
    /// </summary>
    IWorkflowPipeline CreateWorkflowPipeline();
}
