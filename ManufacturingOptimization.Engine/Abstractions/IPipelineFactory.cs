namespace ManufacturingOptimization.Engine.Abstractions;

/// <summary>
/// Factory for creating workflow processing pipelines.
/// </summary>
public interface IPipelineFactory
{
    /// <summary>
    /// Creates a new workflow pipeline instance.
    /// </summary>
    IPipeline CreateWorkflowPipeline();
}
