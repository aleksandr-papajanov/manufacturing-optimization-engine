using ManufacturingOptimization.Common.Messaging.Abstractions;
using System;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;

public class ProviderQuoteResponse : IMessage
{
    public Guid MessageId { get; set; } = Guid.NewGuid();
    public Guid CorrelationId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Guid RequestId { get; set; }
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
}