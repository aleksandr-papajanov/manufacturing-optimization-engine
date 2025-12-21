using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;

public class OptimizationPlanCreatedEvent : IMessage, IEvent
{
    public Guid CommandId { get; set; }
    public Guid ProviderId { get; set; }
    public string Response { get; set; } = string.Empty;
}
