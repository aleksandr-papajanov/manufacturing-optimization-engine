using ManufacturingOptimization.Common.Messaging.Abstractions;
using Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;

public class ProviderRegisteredEvent : IMessage
{
    public Guid ProviderId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string ProviderType { get; set; } = string.Empty;
    
    /// <summary>
    /// List of capabilities this provider supports.
    /// E.g., ["Cleaning", "Disassembly", "Redesign", "Turning", "Grinding", "PartSubstitution", "Reassembly", "Certification"]
    /// </summary>
    public List<string> Capabilities { get; set; } = new();
    
    /// <summary>
    /// Technical requirements/capabilities of the provider.
    /// </summary>
    public TechnicalCapabilities TechnicalCapabilities { get; set; } = new();
}