using Common.Models;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

namespace ManufacturingOptimization.ProviderSimulator.Abstractions;

public interface IProviderSimulator
{
    Guid ProviderId { get; }
    string ProviderName { get; }
    List<ProcessCapability> ProcessCapabilities { get; }
    public ProviderTechnicalCapabilities TechnicalCapabilities { get; }
    ProcessEstimatedEvent HandleEstimateRequest(RequestProcessEstimateCommand request);
}