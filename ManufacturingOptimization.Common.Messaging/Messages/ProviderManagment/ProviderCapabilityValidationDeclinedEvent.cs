using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;

/// <summary>
/// Event indicating provider capability validation was declined.
/// Published by Engine, consumed by ProviderRegistry.
/// </summary>
public class ProviderCapabilityValidationDeclinedEvent : IMessage, IEvent
{
    public Guid CommandId { get; set; }
    public Guid ProviderId { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
