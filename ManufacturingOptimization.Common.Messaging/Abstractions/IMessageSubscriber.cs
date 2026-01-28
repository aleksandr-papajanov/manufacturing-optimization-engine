using System.Threading.Tasks;

namespace ManufacturingOptimization.Common.Messaging.Abstractions;

public interface IMessageSubscriber
{
    void Subscribe<T>(string queueName, Action<T> handler) where T : IMessage;

    Task SubscribeAsync<T>(string exchange, string routingKey, Func<T, Task> handler, string queueName = "") 
        where T : class, IMessage;
}
