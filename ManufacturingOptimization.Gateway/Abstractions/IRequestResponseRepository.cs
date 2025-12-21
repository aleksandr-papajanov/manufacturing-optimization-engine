using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Gateway.Abstractions;

public interface IRequestResponseRepository
{
    void AddRequest(ICommand request);
    void AddResponse(IEvent response);
    IEvent? GetByCommandId(Guid messageId);
}