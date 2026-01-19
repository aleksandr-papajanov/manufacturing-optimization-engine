using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;

public class RequestOptimizationPlanCommand : BaseCommand
{
    /// <summary>
    /// Motor request submitted by customer.
    /// </summary>
    public OptimizationRequest Request { get; set; } = new();
}