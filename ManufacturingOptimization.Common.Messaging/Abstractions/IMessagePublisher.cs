namespace ManufacturingOptimization.Common.Messaging.Abstractions;

public interface IMessagePublisher
{
    void Publish<T>(string exchangeName, string routingKey, T message) where T : IMessage;
}
