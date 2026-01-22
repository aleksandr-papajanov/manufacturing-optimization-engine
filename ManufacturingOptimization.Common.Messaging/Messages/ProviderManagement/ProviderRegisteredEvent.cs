using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;

/// <summary>
/// Event published when a provider has been registered in the system.
/// Contains complete provider information including capabilities.
/// </summary>
public class ProviderRegisteredEvent : BaseEvent
{
    /// <summary>
    /// The registered provider with all capabilities and specifications.
    /// </summary>
    public ProviderModel Provider { get; set; } = new();
}