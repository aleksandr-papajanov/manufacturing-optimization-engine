using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;

/// <summary>
/// Response event for provider capability validation.
/// </summary>
public class ProviderCapabilityValidatedEvent : BaseEvent
{
    public Guid ProviderId { get; set; }
    
    /// <summary>
    /// True if validation approved, false if declined.
    /// </summary>
    public bool IsApproved { get; set; }
    
    /// <summary>
    /// Reason for declining (if IsApproved = false).
    /// </summary>
    public string? Reason { get; set; }
}