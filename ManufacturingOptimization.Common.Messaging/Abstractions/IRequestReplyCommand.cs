namespace ManufacturingOptimization.Common.Messaging.Abstractions;

/// <summary>
/// Represents a command that expects a reply (request-reply pattern).
/// Contains ReplyTo queue name for directing the response.
/// </summary>
public interface IRequestReplyCommand : ICommand
{
    /// <summary>
    /// Queue name where the reply should be sent.
    /// </summary>
    string ReplyTo { get; set; }
}
