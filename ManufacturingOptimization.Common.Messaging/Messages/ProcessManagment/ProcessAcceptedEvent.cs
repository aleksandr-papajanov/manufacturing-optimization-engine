using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;

public class ProcessAcceptedEvent : IMessage, IEvent
{
    public Guid CommandId { get; set; }
    public Guid ProviderId { get; set; }
}
