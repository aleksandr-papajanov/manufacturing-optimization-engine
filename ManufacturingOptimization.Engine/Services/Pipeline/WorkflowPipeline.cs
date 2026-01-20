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
    private DateTime _startTime = default;
    private DateTime _endTime = default;

    public TimeSpan Duration => _startTime == default || _endTime == default
        ? TimeSpan.Zero
        : _endTime - _startTime;

    public WorkflowPipeline(IEnumerable<IWorkflowStep> steps, ILogger<WorkflowPipeline> logger)
    {
        _steps = steps;
        _logger = logger;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        _startTime = DateTime.UtcNow;
        _logger.LogInformation($"Pipeline started. Request {context.Request.RequestId}");

        try
        {
            foreach (var step in _steps)
            {
                await step.ExecuteAsync(context, cancellationToken);
            }

            _endTime = DateTime.UtcNow;
            _logger.LogInformation($"Pipeline finished in {Duration.TotalMilliseconds}ms. Request {context.Request.RequestId}");
        }
        catch (Exception ex)
        {
            _endTime = DateTime.UtcNow;

            _logger.LogError(ex, $"Pipeline failed in {Duration.TotalMilliseconds}ms. Request {context.Request.RequestId}, Error: {ex.Message}");
        }
    }
}
