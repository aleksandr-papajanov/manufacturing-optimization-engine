using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProviderSimulator
{
    /// <summary>
    /// Provider information with all capabilities and specifications.
    /// </summary>
    ProviderModel Provider { get; }
    
    /// <summary>
    /// Handle a process proposal - provider can accept with estimates or decline.
    /// </summary>
    ProposalModel HandleProposal(ProposeProcessToProviderCommand proposal);

    /// <summary>
    /// Handle final confirmation of an accepted proposal.
    /// </summary>
    void HandleConfirmation(ProposalEntity proposalEntity);
}