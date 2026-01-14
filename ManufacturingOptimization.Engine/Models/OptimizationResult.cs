namespace ManufacturingOptimization.Engine.Models;

public class OptimizationResult
{
    public string ProviderId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    
    // The "Score" determines the ranking (Higher is better)
    public double MatchScore { get; set; }

    // Metrics (calculated in T2, placeholders for now)
    public decimal EstimatedCost { get; set; }
    public double EstimatedLeadTimeDays { get; set; }
    public double SustainabilityRating { get; set; } 
}