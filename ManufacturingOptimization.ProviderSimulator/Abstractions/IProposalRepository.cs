using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

/// <summary>
/// Repository for managing process proposals throughout their lifecycle.
/// </summary>
public interface IProposalRepository
{
    /// <summary>
    /// Add a new proposal.
    /// </summary>
    void Add(Proposal proposal);
    
    /// <summary>
    /// Get proposal by ID.
    /// </summary>
    Proposal? GetById(Guid proposalId);
    
    /// <summary>
    /// Get all proposals for a specific provider.
    /// </summary>
    IReadOnlyList<Proposal> GetByProviderId(Guid providerId);
    
    /// <summary>
    /// Get all proposals for a specific plan.
    /// </summary>
    IReadOnlyList<Proposal> GetByPlanId(Guid planId);
    
    /// <summary>
    /// Get proposals by status for a specific provider.
    /// </summary>
    IReadOnlyList<Proposal> GetByProviderIdAndStatus(Guid providerId, ProposalStatus status);
    
    /// <summary>
    /// Get all proposals.
    /// </summary>
    IReadOnlyList<Proposal> GetAll();
    
    /// <summary>
    /// Update proposal status.
    /// </summary>
    void UpdateStatus(Guid proposalId, ProposalStatus newStatus);
    
    /// <summary>
    /// Update proposal to confirmed state.
    /// </summary>
    void ConfirmProposal(Guid proposalId, Guid planId, DateTime confirmedAt, DateTime? scheduledStartTime);
    
    /// <summary>
    /// Delete proposal.
    /// </summary>
    bool Delete(Guid proposalId);
}
