namespace ManufacturingOptimization.Common.Models.DTOs;

public class StrategySelectionResponseDto
{
    public string Status { get; set; } = string.Empty;

    public Guid RequestId { get; set; }
    public Guid StrategyId { get; set; }
}
