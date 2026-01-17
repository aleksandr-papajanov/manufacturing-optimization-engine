using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;

/// <summary>
/// Event published when a service has completed initialization and is ready to receive messages.
/// </summary>
public class ServiceReadyEvent : IMessage
{
    public string ServiceName { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public List<string> SubscribedQueues { get; set; } = new();
}
