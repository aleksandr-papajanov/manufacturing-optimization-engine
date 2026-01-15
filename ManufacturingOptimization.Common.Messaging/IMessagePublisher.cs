using System.Threading.Tasks;

namespace Common.Messaging
{
    public interface IMessagePublisher
    {
        Task PublishAsync<T>(T message);
    }
}