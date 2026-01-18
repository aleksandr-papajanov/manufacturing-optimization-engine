using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;

/// <summary>
/// Command to validate provider capabilities before registration/startup.
/// Published by ProviderRegistry, consumed by Engine using RPC pattern.
/// </summary>
public class ValidateProviderCapabilityCommand : IMessage, ICommand
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
    public Guid ProviderId { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public List<ProviderProcessCapability> ProcessCapabilities { get; set; } = [];
    public ProviderTechnicalCapabilities TechnicalCapabilities { get; set; } = new();
    
    /// <summary>
    /// RPC: Queue name where response should be sent.
    /// </summary>
    public string ReplyTo { get; set; } = string.Empty;
}