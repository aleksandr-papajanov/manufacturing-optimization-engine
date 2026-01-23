namespace ManufacturingOptimization.Common.Messaging.Abstractions;

/// <summary>
/// Base interface for all messages in the system.
/// </summary>
public interface IMessage
{
    /// <summary>
    /// Timestamp when the message was created.
    /// </summary>
    DateTime Timestamp { get; set; }
}