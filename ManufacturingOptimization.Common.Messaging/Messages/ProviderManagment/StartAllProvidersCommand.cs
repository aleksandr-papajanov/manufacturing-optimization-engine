using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;

public class StartAllProvidersCommand : IMessage
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
}