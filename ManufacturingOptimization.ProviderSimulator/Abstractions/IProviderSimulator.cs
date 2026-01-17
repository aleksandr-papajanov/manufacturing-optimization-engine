using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProviderSimulator
{
    Guid ProviderId { get; }
    string ProviderName { get; }
    List<string> Capabilities { get; }
    
    double AxisHeight { get; }
    double Power { get; }
    double Tolerance { get; }

    bool HandleProposal(ProposeProcessCommand proposal);
}