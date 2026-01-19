namespace ManufacturingOptimization.Common.Messaging.Abstractions;

/// <summary>
/// Represents an event - something that has happened in the system.
/// Events can be correlated to the command that triggered them.
/// </summary>
public interface IEvent : IMessage
{
    /// <summary>
    /// Unique identifier for this event.
    /// </summary>
    Guid EventId { get; set; }
    
    /// <summary>
    /// ID of the command that triggered this event (for correlation/causation tracking).
    /// Optional - some events may not be triggered by commands.
    /// </summary>
    Guid? CorrelationId { get; set; }
}
