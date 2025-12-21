using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.GatewayAPI.Abstractions;

public interface IRequestResponseRepository
{
    void AddRequest(IMessage request);
    void AddResponse(IMessage response);
    IMessage? GetByMessageId(Guid messageId);
}
