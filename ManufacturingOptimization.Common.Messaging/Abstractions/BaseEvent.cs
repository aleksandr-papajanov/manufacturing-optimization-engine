namespace ManufacturingOptimization.Common.Messaging.Abstractions;

/// <summary>
/// Base class for events. Provides default implementation of IEvent.
/// </summary>
public abstract class BaseEvent : IEvent
{
    public Guid EventId { get; set; } = Guid.NewGuid();
    public Guid? CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
