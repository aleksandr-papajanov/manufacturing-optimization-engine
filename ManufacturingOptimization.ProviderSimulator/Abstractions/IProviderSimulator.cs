using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProviderSimulator
{
    Guid ProviderId { get; }
    string ProviderName { get; }
    List<ProviderProcessCapability> ProcessCapabilities { get; }
    ProviderTechnicalCapabilities TechnicalCapabilities { get; }
    ProcessEstimatedEvent HandleEstimateRequest(RequestProcessEstimateCommand request);
}