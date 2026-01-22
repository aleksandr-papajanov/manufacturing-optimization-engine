namespace ManufacturingOptimization.Common.Models.DTOs;

/// <summary>
/// Response returned when a strategy is selected.
/// </summary>
public class StrategySelectionResponse
{
    /// <summary>
    /// Status message indicating selection was received.
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// The request ID for which the strategy was selected.
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// The ID of the selected strategy.
    /// </summary>
    public Guid StrategyId { get; set; }
}
