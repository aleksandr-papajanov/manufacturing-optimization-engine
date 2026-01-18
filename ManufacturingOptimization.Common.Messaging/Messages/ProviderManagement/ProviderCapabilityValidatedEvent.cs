using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;

/// <summary>
/// Unified response for provider capability validation.
/// Replaces separate Approved/Declined events.
/// </summary>
public class ProviderCapabilityValidatedEvent : IMessage, IEvent
{
    public Guid ProviderId { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    
    /// <summary>
    /// True if validation approved, false if declined.
    /// </summary>
    public bool IsApproved { get; set; }
    
    /// <summary>
    /// Reason for declining (if IsApproved = false).
    /// Empty if approved.
    /// </summary>
    public string? Reason { get; set; }
    
    /// <summary>
    /// ID of the command that triggered this response (for correlation).
    /// </summary>
    public Guid CommandId { get; set; }
}
