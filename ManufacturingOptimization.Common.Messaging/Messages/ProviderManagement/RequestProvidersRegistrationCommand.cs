using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;

public class RequestProvidersRegistrationCommand : IMessage
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
}