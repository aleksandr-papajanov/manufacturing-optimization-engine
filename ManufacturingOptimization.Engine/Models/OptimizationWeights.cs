namespace ManufacturingOptimization.Engine.Models;


/// <summary>
/// Weights for multi-objective optimization.
/// </summary>
public class OptimizationWeights
{
    public double CostWeight { get; set; }
    public double TimeWeight { get; set; }
    public double QualityWeight { get; set; }
    public double EmissionsWeight { get; set; }
}
