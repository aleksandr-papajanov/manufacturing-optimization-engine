using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.ProviderSimulator.Models;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProviderSimulator
{
    /// <summary>
    /// Provider information with all capabilities and specifications.
    /// </summary>
    Provider Provider { get; }
    
    /// <summary>
    /// Handle a process proposal - provider can accept with estimates or decline.
    /// </summary>
    ProcessProposalEstimatedEvent HandleProposal(ProposeProcessToProviderCommand proposal);
    
    /// <summary>
    /// Handle final confirmation of an accepted proposal.
    /// </summary>
    ProcessProposalConfirmedEvent HandleConfirmation(ConfirmProcessProposalCommand confirmation);
}