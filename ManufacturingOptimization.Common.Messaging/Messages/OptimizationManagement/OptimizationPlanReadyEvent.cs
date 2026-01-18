using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;

/// <summary>
/// Published when optimization plan is ready with provider assignments.
/// Contains complete workflow with selected providers and cost/time/quality metrics.
/// </summary>
public class OptimizationPlanReadyEvent : IMessage, IEvent
{
    /// <summary>
    /// Command that triggered this event (for correlation).
    /// </summary>
    public Guid CommandId { get; set; }
    
    /// <summary>
    /// Optimized plan with all details.
    /// </summary>
    public OptimizationPlan Plan { get; set; } = new();
}
