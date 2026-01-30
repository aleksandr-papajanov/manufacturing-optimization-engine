using ManufacturingOptimization.Common.Messaging.Abstractions;
using System;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;

public class ProviderQuoteRequest : IRequestReplyCommand
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public Guid CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Guid CommandId { get; set; } = Guid.NewGuid();
    public string ReplyTo { get; set; } = string.Empty;

    public Guid RequestId { get; set; }
    public string ProcessName { get; set; } = string.Empty;
}