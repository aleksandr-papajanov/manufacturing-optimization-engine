namespace ManufacturingOptimization.Common.Messaging.Abstractions;

/// <summary>
/// Represents a command - an instruction to perform an action.
/// Commands have a unique ID for tracking and correlation.
/// </summary>
public interface ICommand : IMessage
{
    /// <summary>
    /// Unique identifier for this command.
    /// </summary>
    Guid CommandId { get; set; }
}
