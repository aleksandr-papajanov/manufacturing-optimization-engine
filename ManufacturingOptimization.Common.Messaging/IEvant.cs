using System;

namespace Common.Messaging
{
    public interface IEvent
    {
        Guid EventId { get; }
        DateTime Timestamp { get; }
    }
}