using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;

public class RequestOptimizationPlanCommand : BaseCommand
{
    /// <summary>
    /// Motor request submitted by customer.
    /// </summary>
    public OptimizationRequestModel Request { get; set; } = new();
}