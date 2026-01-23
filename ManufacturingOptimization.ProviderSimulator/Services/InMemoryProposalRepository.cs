using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Services;

/// <summary>
/// In-memory implementation of proposal repository.
/// Thread-safe for concurrent access.
/// </summary>
public class InMemoryProposalRepository : IProposalRepository
{
    private readonly Dictionary<Guid, Proposal> _proposals = new();
    private readonly object _lock = new();

    public void Add(Proposal proposal)
    {
        lock (_lock)
        {
            _proposals[proposal.ProposalId] = proposal;
        }
    }

    public Proposal? GetById(Guid proposalId)
    {
        lock (_lock)
        {
            return _proposals.TryGetValue(proposalId, out var proposal) ? proposal : null;
        }
    }

    public IReadOnlyList<Proposal> GetByProviderId(Guid providerId)
    {
        lock (_lock)
        {
            return _proposals.Values
                .Where(p => p.ProviderId == providerId)
                .OrderByDescending(p => p.AcceptedAt)
                .ToList()
                .AsReadOnly();
        }
    }

    public IReadOnlyList<Proposal> GetByPlanId(Guid planId)
    {
        lock (_lock)
        {
            return _proposals.Values
                .Where(p => p.PlanId == planId)
                .OrderBy(p => p.Process)
                .ToList()
                .AsReadOnly();
        }
    }
    
    public IReadOnlyList<Proposal> GetByProviderIdAndStatus(Guid providerId, ProposalStatus status)
    {
        lock (_lock)
        {
            return _proposals.Values
                .Where(p => p.ProviderId == providerId && p.Status == status)
                .OrderByDescending(p => p.AcceptedAt)
                .ToList()
                .AsReadOnly();
        }
    }

    public IReadOnlyList<Proposal> GetAll()
    {
        lock (_lock)
        {
            return _proposals.Values
                .OrderByDescending(p => p.AcceptedAt)
                .ToList()
                .AsReadOnly();
        }
    }

    public void UpdateStatus(Guid proposalId, ProposalStatus newStatus)
    {
        lock (_lock)
        {
            if (_proposals.TryGetValue(proposalId, out var proposal))
            {
                proposal.Status = newStatus;
            }
        }
    }
    
    public void ConfirmProposal(Guid proposalId, Guid planId, DateTime confirmedAt, DateTime? scheduledStartTime)
    {
        lock (_lock)
        {
            if (_proposals.TryGetValue(proposalId, out var proposal))
            {
                proposal.Status = ProposalStatus.Confirmed;
                proposal.PlanId = planId;
                proposal.ConfirmedAt = confirmedAt;
                proposal.ScheduledStartTime = scheduledStartTime;
            }
        }
    }

    public bool Delete(Guid proposalId)
    {
        lock (_lock)
        {
            return _proposals.Remove(proposalId);
        }
    }
}
