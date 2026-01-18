using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

public class ProposeProcessCommand : IMessage, ICommand
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
    public Guid ProviderId { get; set; }
}