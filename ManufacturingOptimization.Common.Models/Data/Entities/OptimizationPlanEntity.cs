namespace ManufacturingOptimization.Common.Models.Data.Entities;

/// <summary>
/// Optimization plan entity for database storage.
/// </summary>
public class OptimizationPlanEntity
{
    public Guid Id { get; set; }
    public Guid RequestId { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? SelectedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }

    // Navigation property
    public OptimizationStrategyEntity? SelectedStrategy { get; set; }
}
