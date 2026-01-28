using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;

/// <summary>
/// Published when optimization plan status changes during pipeline execution.
/// Allows tracking progress of optimization in real-time.
/// </summary>
public class OptimizationPlanUpdatedEvent : BaseEvent
{
    /// <summary>
    /// Plan ID.
    /// </summary>
    public OptimizationPlanModel Plan { get; set; } = null!;
}
