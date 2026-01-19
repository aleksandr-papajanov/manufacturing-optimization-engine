using ManufacturingOptimization.Common.Models;
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
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly IMessagePublisher _messagePublisher;

    public string Name => "Plan Persistence";

    public PlanPersistenceStep(
        IOptimizationPlanRepository planRepository,
        IMessagePublisher messagePublisher)
    {
        _planRepository = planRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (context.SelectedStrategy == null)
        {
            throw new InvalidOperationException("Cannot persist plan: no strategy selected");
        }

        if (!context.PlanId.HasValue)
        {
            throw new InvalidOperationException("Cannot persist plan: Plan ID is missing in context");
        }

        // Create optimization plan from context with pre-assigned PlanId
        var plan = new OptimizationPlan
        {
            PlanId = context.PlanId.Value,
            RequestId = context.Request.RequestId,
            SelectedStrategy = context.SelectedStrategy,
            Status = OptimizationPlanStatus.Confirmed, // Changed to Confirmed after provider confirmations
            CreatedAt = DateTime.UtcNow,
            SelectedAt = DateTime.UtcNow
        };

        // Save to repository
        var savedPlan = _planRepository.Create(plan);
        
        // Store in context for subsequent steps
        context.SavedPlan = savedPlan;

        // Publish plan ready event
        var planReadyEvent = new OptimizationPlanReadyEvent
        {
            CorrelationId = context.Request.RequestId,
            Plan = savedPlan
        };

        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanReady, planReadyEvent);

        await Task.CompletedTask;
    }
}
