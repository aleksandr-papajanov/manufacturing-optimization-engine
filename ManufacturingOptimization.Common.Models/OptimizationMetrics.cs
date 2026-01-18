namespace ManufacturingOptimization.Common.Models;

/// <summary>
/// Optimization metrics and results common to both optimization calculation and strategy representation.
/// Contains all quantitative outcomes from the optimization process.
/// </summary>
public class OptimizationMetrics
{
    /// <summary>
    /// Total estimated cost for the entire workflow.
    /// </summary>
    public decimal TotalCost { get; set; }
    
    /// <summary>
    /// Total estimated duration for the entire workflow.
    /// </summary>
    public TimeSpan TotalDuration { get; set; }
    
    /// <summary>
    /// Average quality score across all steps (0.0 - 1.0).
    /// </summary>
    public double AverageQuality { get; set; }
    
    /// <summary>
    /// Total estimated carbon emissions in kg CO2.
    /// </summary>
    public double TotalEmissionsKgCO2 { get; set; }
    
    /// <summary>
    /// Optimization solver status (OPTIMAL, FEASIBLE, INFEASIBLE, etc.).
    /// </summary>
    public string SolverStatus { get; set; } = string.Empty;
    
    /// <summary>
    /// Objective function value from the solver (for debugging/analysis).
    /// </summary>
    public double ObjectiveValue { get; set; }
}
