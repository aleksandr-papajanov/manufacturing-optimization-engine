using System;

namespace ManufacturingOptimization.Common.Models.DTOs;

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
