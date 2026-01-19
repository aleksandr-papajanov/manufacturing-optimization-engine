using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.ProviderSimulator.Models;

/// <summary>
/// Represents a process proposal in the provider's system.
/// Tracks proposals from acceptance through completion.
/// </summary>
public class Proposal
{
    public Guid ProposalId { get; set; } = Guid.NewGuid();
    public Guid ProviderId { get; set; }
    public Guid RequestId { get; set; }
    public Guid? PlanId { get; set; }
    public ProcessType Process { get; set; }
    
    /// <summary>
    /// When the proposal was first accepted by the provider.
    /// </summary>
    public DateTime AcceptedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// When the proposal was confirmed (after strategy selection).
    /// </summary>
    public DateTime? ConfirmedAt { get; set; }
    
    /// <summary>
    /// Scheduled start time for the process.
    /// </summary>
    public DateTime? ScheduledStartTime { get; set; }
    
    /// <summary>
    /// Current status of the proposal.
    /// </summary>
    public ProposalStatus Status { get; set; } = ProposalStatus.Accepted;
    
    /// <summary>
    /// Estimate provided when accepting the proposal.
    /// </summary>
    public ProcessEstimate? Estimate { get; set; }
}

/// <summary>
/// Status of a proposal throughout its lifecycle.
/// </summary>
public enum ProposalStatus
{
    /// <summary>
    /// Proposal accepted by provider with preliminary estimate.
    /// </summary>
    Accepted,
    
    /// <summary>
    /// Proposal confirmed after customer selected the strategy.
    /// </summary>
    Confirmed,
    
    /// <summary>
    /// Work in progress.
    /// </summary>
    InProgress,
    
    /// <summary>
    /// Process completed.
    /// </summary>
    Completed,
    
    /// <summary>
    /// Proposal declined or cancelled.
    /// </summary>
    Cancelled
}
