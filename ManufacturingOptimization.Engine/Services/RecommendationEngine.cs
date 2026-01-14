using Common.Models; // Namespace for MotorRequest and Enums
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services;

public class RecommendationEngine : IRecommendationEngine
{
    public List<OptimizationResult> GenerateRecommendations(MotorRequest request, IEnumerable<Provider> capableProviders)
    {
        var results = new List<OptimizationResult>();

        foreach (var provider in capableProviders)
        {
            // 1. Calculate Metrics (Simulated for T1)
            var result = new OptimizationResult
            {
                ProviderId = provider.Id,
                ProviderName = provider.Name ?? "Unknown Provider",
                EstimatedCost = CalculateBaseCost(provider),
                EstimatedLeadTimeDays = CalculateLeadTime(provider),
                SustainabilityRating = GetSustainabilityScore(provider)
            };

            // 2. Calculate Match Score
            result.MatchScore = CalculateScore(request.Constraints.Priority, result);

            results.Add(result);
        }

        // 3. Return Ranked List (Highest Score First)
        return results.OrderByDescending(r => r.MatchScore).ToList();
    }

    private double CalculateScore(OptimizationPriority priority, OptimizationResult metrics)
    {
        switch (priority)
        {
            case OptimizationPriority.LowestCost:
                // Lower cost = Higher score
                return 10000 / (double)(metrics.EstimatedCost + 1);

            case OptimizationPriority.FastestDelivery:
                // Lower time = Higher score
                return 100 / (metrics.EstimatedLeadTimeDays + 1);

            case OptimizationPriority.HighestQuality:
            // Removed "HighestEfficiency" case as it does not exist in your Enum
            default:
                // Higher rating = Higher score
                return metrics.SustainabilityRating * 10;
        }
    }

    // --- Helpers (Mock Logic for T1) ---
    private decimal CalculateBaseCost(Provider provider) => 1000 + (provider.Id.Length * 50);
    private double CalculateLeadTime(Provider provider) => 5 + (provider.Id.Length % 15);
    private double GetSustainabilityScore(Provider provider) => 1 + (provider.Id.Length % 9);
}