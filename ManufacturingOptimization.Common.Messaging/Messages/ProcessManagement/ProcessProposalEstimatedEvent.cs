using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Response from provider regarding process proposal.
/// </summary>
public class ProcessProposalEstimatedEvent : BaseEvent
{
    public Guid RequestId { get; set; }
    public Guid ProviderId { get; set; }
    public ProcessType Process { get; set; }
    
    /// <summary>
    /// Whether the provider accepts the proposal.
    /// </summary>
    public bool IsAccepted { get; set; }
    
    /// <summary>
    /// Preliminary estimate (only present if accepted).
    /// </summary>
    public ProcessEstimate? Estimate { get; set; }
    
    /// <summary>
    /// Reason for declining (only present if rejected).
    /// </summary>
    public string? DeclineReason { get; set; }
    
    /// <summary>
    /// Additional notes from provider.
    /// </summary>
    public string Notes { get; set; } = string.Empty;
}
