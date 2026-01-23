using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;

/// <summary>
/// Published when optimization plan is ready with provider assignments.
/// Contains complete workflow with selected providers and cost/time/quality metrics.
/// </summary>
public class OptimizationPlanReadyEvent : BaseEvent
{
    /// <summary>
    /// Optimized plan with all details.
    /// </summary>
    public OptimizationPlanModel Plan { get; set; } = new();
}
