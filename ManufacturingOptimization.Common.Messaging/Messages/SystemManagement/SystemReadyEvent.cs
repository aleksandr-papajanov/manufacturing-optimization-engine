using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;

/// <summary>
/// Event published when all services are ready and system can start processing.
/// </summary>
public class SystemReadyEvent : BaseEvent
{
    public List<string> ReadyServices { get; set; } = new();
}
