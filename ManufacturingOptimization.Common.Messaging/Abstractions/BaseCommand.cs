namespace ManufacturingOptimization.Common.Messaging.Abstractions;

/// <summary>
/// Base class for commands. Provides default implementation of ICommand.
/// </summary>
public abstract class BaseCommand : ICommand
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
