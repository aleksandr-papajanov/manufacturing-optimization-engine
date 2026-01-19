using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Simple pipeline executor - runs steps sequentially.
/// </summary>
public class WorkflowPipeline : IWorkflowPipeline
{
    private readonly IEnumerable<IWorkflowStep> _steps;
    private readonly ILogger<WorkflowPipeline> _logger;

    public WorkflowPipeline(IEnumerable<IWorkflowStep> steps, ILogger<WorkflowPipeline> logger)
    {
        _steps = steps;
        _logger = logger;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        _logger.LogInformation(
            "Starting workflow pipeline for request {RequestId}. Workflow type: {WorkflowType}",
            context.Request.RequestId, context.WorkflowType ?? "Unknown");

        try
        {
            foreach (var step in _steps)
            {
                await step.ExecuteAsync(context, cancellationToken);
            }

            var duration = DateTime.UtcNow - startTime;
            _logger.LogInformation(
                "Pipeline completed successfully in {Duration}ms. Request {RequestId}, Workflow: {WorkflowType}, Strategies: {StrategyCount}, Selected: {SelectedPriority}, PlanId: {PlanId}",
                duration.TotalMilliseconds,
                context.Request.RequestId,
                context.WorkflowType,
                context.Strategies.Count,
                context.SelectedStrategy?.Priority.ToString() ?? "None",
                context.PlanId?.ToString() ?? "None");
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            _logger.LogError(ex,
                "Pipeline failed in {Duration}ms. Request {RequestId}, Error: {ErrorMessage}",
                duration.TotalMilliseconds,
                context.Request.RequestId,
                ex.Message);
        }
    }
}
