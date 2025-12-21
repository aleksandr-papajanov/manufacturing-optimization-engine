using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProviderSimulator
{
    Guid ProviderId { get; }
    string ProviderName { get; }

    bool HandleProposal(ProposeProcessCommand proposal);
}