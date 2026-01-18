using Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Saves the selected optimization plan to the repository and publishes completion event.
/// This step runs after the customer has selected their preferred strategy.
/// </summary>
public class PlanPersistenceStep : IWorkflowStep
{
    private readonly ILogger<PlanPersistenceStep> _logger;
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly IMessagePublisher _messagePublisher;

    public string Name => "Plan Persistence";

    public PlanPersistenceStep(
        ILogger<PlanPersistenceStep> logger,
        IOptimizationPlanRepository planRepository,
        IMessagePublisher messagePublisher)
    {
        _logger = logger;
        _planRepository = planRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (context.SelectedStrategy == null)
        {
            context.Errors.Add("Cannot persist plan: no strategy selected");
            return;
        }

        try
        {
            // Create optimization plan from context
            var plan = new OptimizationPlan
            {
                RequestId = context.Request.RequestId,
                SelectedStrategy = context.SelectedStrategy,
                Status = OptimizationPlanStatus.Selected,
                CreatedAt = DateTime.UtcNow,
                SelectedAt = DateTime.UtcNow,
                IsSuccess = context.IsSuccess,
                Errors = context.Errors.ToList()
            };

            // Save to repository
            var savedPlan = _planRepository.Create(plan);

            // Publish plan ready event
            var planReadyEvent = new OptimizationPlanReadyEvent
            {
                CommandId = Guid.NewGuid(),
                Plan = savedPlan
            };

            _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanReady, planReadyEvent);
        }
        catch (Exception ex)
        {
            context.Errors.Add($"Failed to persist plan: {ex.Message}");
        }

        await Task.CompletedTask;
    }
}
