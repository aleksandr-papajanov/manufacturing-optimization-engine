using ManufacturingOptimization.Common.Models.Contracts;

namespace ManufacturingOptimization.Common.Models.DTOs;

/// <summary>
/// Response wrapper for optimization strategies with readiness status.
/// </summary>
public class StrategiesResponseDto
{
    /// <summary>
    /// Indicates whether strategies are ready for customer selection.
    /// </summary>
    public bool IsReady { get; set; }
    
    /// <summary>
    /// List of available optimization strategies.
    /// Null or empty when IsReady is false.
    /// </summary>
    public List<OptimizationStrategyModel> Strategies { get; set; } = new();
    
    /// <summary>
    /// Status message: "Ready" or "Processing".
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
