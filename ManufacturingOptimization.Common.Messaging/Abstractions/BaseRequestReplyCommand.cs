namespace ManufacturingOptimization.Common.Messaging.Abstractions;

/// <summary>
/// Base class for request-reply commands. Provides default implementation of IRequestReplyCommand.
/// </summary>
public abstract class BaseRequestReplyCommand : BaseCommand, IRequestReplyCommand
{
    public string ReplyTo { get; set; } = string.Empty;
}
