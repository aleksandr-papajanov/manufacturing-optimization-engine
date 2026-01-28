using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Enums;

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
    
    // Stores the frozen strategy as JSON so it doesn't change during execution
    public string StrategyJson { get; set; } = "{}";

    // Helper to access the strategy as a real object
    [NotMapped]
    public OptimizationStrategyModel Strategy
    {
        get => string.IsNullOrEmpty(StrategyJson) 
               ? new OptimizationStrategyModel() 
               : JsonSerializer.Deserialize<OptimizationStrategyModel>(StrategyJson) ?? new OptimizationStrategyModel();
        set => StrategyJson = JsonSerializer.Serialize(value);
    }

    // Helper to handle Status as an Enum
    [NotMapped]
    public OptimizationPlanStatus StatusEnum
    {
        get => Enum.TryParse<OptimizationPlanStatus>(Status, out var s) ? s : OptimizationPlanStatus.Draft;
        set => Status = value.ToString();
    }
}
