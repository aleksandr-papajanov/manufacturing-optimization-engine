using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProviderSimulator
{
    ProviderModel Provider { get; }

    Task<ProposalModel> HandleProposalAsync(ProposeProcessToProviderCommand proposal);
    void HandleConfirmation(ProposalEntity proposalEntity);
    Task<List<TimeWindowModel>> GetWorkingSegmentsAsync(TimeWindowModel allocatedSlot);
    Task<List<TimeSegmentModel>> GetAllSegmentsAsync(TimeWindowModel allocatedSlot);
}