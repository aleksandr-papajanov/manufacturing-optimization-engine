namespace ManufacturingOptimization.Engine.Models;

/// <summary>
/// Results from the optimization step.
/// </summary>
public class OptimizationResult
{
    public decimal TotalCost { get; set; }
    public TimeSpan TotalDuration { get; set; }
    public double AverageQuality { get; set; }
    public double TotalEmissionsKgCO2 { get; set; }
    public string SolverStatus { get; set; } = string.Empty;
    public double ObjectiveValue { get; set; }
}