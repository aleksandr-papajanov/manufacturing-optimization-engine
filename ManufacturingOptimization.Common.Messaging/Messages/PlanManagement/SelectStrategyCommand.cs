using ManufacturingOptimization.Common.Messaging.Abstractions;

namespace ManufacturingOptimization.Common.Messaging.Messages.PlanManagement;

/// <summary>
/// Command sent by customer to select their preferred optimization strategy.
/// </summary>
public class SelectStrategyCommand : BaseCommand
{
    /// <summary>
    /// The original request ID.
    /// </summary>
    public Guid RequestId { get; set; }
    
    /// <summary>
    /// The ID of the selected strategy.
    /// </summary>
    public Guid SelectedStrategyId { get; set; }
    
    /// <summary>
    /// Name of the selected strategy (e.g., "Budget Strategy", "Express Strategy").
    /// </summary>
    public string SelectedStrategyName { get; set; } = string.Empty;
    
    /// <summary>
    /// When the selection was made.
    /// </summary>
    public DateTime SelectedAt { get; set; } = DateTime.UtcNow;
}
