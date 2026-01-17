using ManufacturingOptimization.Engine.Abstractions.Pipeline;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Simple pipeline executor - runs steps sequentially.
/// </summary>
public class WorkflowPipeline : IPipeline
{
    private readonly IEnumerable<IPipelineStep> _steps;
    private readonly ILogger<WorkflowPipeline> _logger;

    public WorkflowPipeline(
        IEnumerable<IPipelineStep> steps,
        ILogger<WorkflowPipeline> logger)
    {
        _steps = steps;
        _logger = logger;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting workflow pipeline for request {RequestId}", context.Request.RequestId);

        foreach (var step in _steps)
        {
            if (!context.IsSuccess)
            {
                _logger.LogWarning("Pipeline stopped due to errors: {Errors}", string.Join(", ", context.Errors));
                return;
            }

            _logger.LogDebug("Executing step: {StepName}", step.Name);
            
            try
            {
                await step.ExecuteAsync(context, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in step {StepName}", step.Name);
                context.Errors.Add($"{step.Name}: {ex.Message}");
                return;
            }
        }

        _logger.LogInformation("Pipeline completed successfully");
    }
}
