namespace ManufacturingOptimization.Gateway.DTOs;

/// <summary>
/// DTO for optimization plan returned to client
/// </summary>
public class OptimizationPlanDto
{
    public Guid PlanId { get; set; }
    public Guid RequestId { get; set; }
    public OptimizationStrategyDto? SelectedStrategy { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SelectedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
}
