namespace ManufacturingOptimization.Common.Messaging.Abstractions;

/// <summary>
/// Event that was triggered by a command. Contains CommandId for correlation.
/// </summary>
public interface IEvent
{
    /// <summary>
    /// ID of the command that triggered this event (for correlation/causation tracking)
    /// </summary>
    Guid CommandId { get; set; }
}
