using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;

public class SelectStrategyCommand : IMessage, ICommand
{
    public Guid CommandId { get; set; } = Guid.NewGuid();
    public Guid RequestId { get; set; }
    public string SelectedProviderId { get; set; } = string.Empty;
    public string SelectedStrategyName { get; set; } = string.Empty; // e.g., "Refurbish" or "Upgrade"
    public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
}