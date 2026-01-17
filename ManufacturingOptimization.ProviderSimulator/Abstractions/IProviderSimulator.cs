using Common.Models;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProviderSimulator
{
    Guid ProviderId { get; }
    string ProviderName { get; }
    List<ProcessCapability> ProcessCapabilities { get; }
    public TechnicalCapabilities TechnicalCapabilities { get; }
    ProcessEstimatedEvent HandleEstimateRequest(RequestProcessEstimateCommand request);
}