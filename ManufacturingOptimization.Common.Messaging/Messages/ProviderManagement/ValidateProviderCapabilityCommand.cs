using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;

/// <summary>
/// Command to validate provider capabilities before registration/startup.
/// Published by ProviderRegistry, consumed by Engine using RPC pattern.
/// </summary>
public class ValidateProviderCapabilityCommand : BaseRequestReplyCommand
{
    /// <summary>
    /// Provider to validate with all capabilities and specifications.
    /// </summary>
    public Provider Provider { get; set; } = new();
}