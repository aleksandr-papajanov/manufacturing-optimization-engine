using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;

/// <summary>
/// Event published when an optimization plan has been created and confirmed.
/// Contains the complete plan with selected strategy and execution details.
/// </summary>
public class OptimizationPlanCreatedEvent : BaseEvent
{
    /// <summary>
    /// The complete optimization plan.
    /// </summary>
    public OptimizationPlan Plan { get; set; } = new();
}
