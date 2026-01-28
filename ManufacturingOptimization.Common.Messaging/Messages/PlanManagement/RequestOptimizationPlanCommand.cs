using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;

public class RequestOptimizationPlanCommand : BaseCommand
{
    public OptimizationRequestModel Request { get; set; } = null!;
    public OptimizationPlanModel Plan { get; set; } = null!;
}