using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

/// <summary>
/// Response from provider regarding process proposal.
/// </summary>
public class ProcessProposalEstimatedEvent : BaseEvent
{
    public ProposalModel Proposal { get; set; } = null!;
}
