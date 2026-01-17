using ManufacturingOptimization.Common.Messaging.Abstractions;
using Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;

/// <summary>
/// Command to validate provider capabilities before registration/startup.
/// Published by ProviderRegistry, consumed by Engine.
/// </summary>
public class ValidateProviderCapabilityCommand : IMessage, ICommand
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
    public Guid ProviderId { get; set; }
    public string ProviderType { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public List<string> Capabilities { get; set; } = new();
    public TechnicalCapabilities TechnicalCapabilities { get; set; } = new();
}