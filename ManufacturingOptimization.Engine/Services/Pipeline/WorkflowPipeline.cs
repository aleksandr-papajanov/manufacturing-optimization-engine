using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

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
        foreach (var step in _steps)
        {
            if (!context.IsSuccess)
            {
                return;
            }

            try
            {
                await step.ExecuteAsync(context, cancellationToken);
            }
            catch (Exception ex)
            {
                context.Errors.Add($"{step.Name}: {ex.Message}");
                return;
            }
        }
    }
}
