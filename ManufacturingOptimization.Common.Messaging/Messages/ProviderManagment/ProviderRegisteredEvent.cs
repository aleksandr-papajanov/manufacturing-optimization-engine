using ManufacturingOptimization.Common.Messaging.Abstractions;
using Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;

public class ProviderRegisteredEvent : IMessage
{
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    
    /// <summary>
    /// List of processes this provider can perform with specific characteristics.
    /// </summary>
    public List<ProcessCapability> ProcessCapabilities { get; set; } = new();
    
    /// <summary>
    /// Technical requirements/capabilities of the provider.
    /// </summary>
    public TechnicalCapabilities TechnicalCapabilities { get; set; } = new();
}