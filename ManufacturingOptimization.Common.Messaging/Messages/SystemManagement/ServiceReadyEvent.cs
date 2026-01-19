using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;

/// <summary>
/// Event published when a service has completed initialization and is ready to receive messages.
/// </summary>
public class ServiceReadyEvent : BaseEvent
{
    public string ServiceName { get; set; } = string.Empty;
}
