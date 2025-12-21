using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;

public class RequestOptimizationPlanCommand : IMessage, ICommand
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
}