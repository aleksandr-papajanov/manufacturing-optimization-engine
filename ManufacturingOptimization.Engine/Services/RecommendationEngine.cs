using Common.Models; 
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services;

public class RecommendationEngine : IRecommendationEngine
{
    public List<OptimizationResult> GenerateRecommendations(MotorRequest request, IEnumerable<Provider> capableProviders)
    {
        var results = new List<OptimizationResult>();

        // 1. Determine the Strategy based on Efficiency Upgrade
        // If Target > Current, it's an UPGRADE. Otherwise, it's REFURBISH/REPAIR.
        var isUpgrade = request.Specs.TargetEfficiency > request.Specs.CurrentEfficiency;
        var strategyName = isUpgrade ? "Upgrade" : "Refurbish";

        foreach (var provider in capableProviders)
        {
            // 2. Calculate Base Estimates (Physics + Market Rates)
            var (baseCost, baseTime, baseEco) = CalculateBaseMetrics(request.Specs.PowerKW, isUpgrade);

            // 3. Apply Provider "Personality" Modifiers
            // (In a real system, these would come from the database/contract)
            ApplyProviderModifiers(provider.Name, ref baseCost, ref baseTime, ref baseEco);

            var result = new OptimizationResult
            {
                ProviderId = provider.Id,
                ProviderName = provider.Name,
                EstimatedCost = baseCost,
                EstimatedLeadTimeDays = baseTime,
                SustainabilityRating = baseEco
            };

            // 4. Calculate Match Score (0 to 100)
            result.MatchScore = CalculateScore(request.Constraints.Priority, result);

            results.Add(result);
        }

        // 5. Return Ranked List (Highest Score First)
        return results.OrderByDescending(r => r.MatchScore).ToList();
    }

    private (decimal cost, double time, double eco) CalculateBaseMetrics(double powerKw, bool isUpgrade)
    {
        if (isUpgrade)
        {
            // Upgrade is expensive and takes time, but better for the planet (IE4)
            decimal cost = 500m + (decimal)(powerKw * 400); // $500 base + $400/kW
            double time = 14 + (powerKw * 0.5);             // 14 days + 0.5 day/kW
            double eco = 9.5;                               // High sustainability
            return (cost, time, eco);
        }
        else
        {
            // Refurbish is cheaper and faster
            decimal cost = 200m + (decimal)(powerKw * 150); // $200 base + $150/kW
            double time = 5 + (powerKw * 0.2);              // 5 days + 0.2 day/kW
            double eco = 6.0;                               // Medium sustainability
            return (cost, time, eco);
        }
    }

    private void ApplyProviderModifiers(string providerName, ref decimal cost, ref double time, ref double eco)
    {
        if (providerName.Contains("Fast")) 
        {
            // Premium Service: Faster but Expensive
            cost *= 1.5m;
            time *= 0.6; 
        }
        else if (providerName.Contains("Budget"))
        {
            // Budget Service: Cheap but Slow
            cost *= 0.7m;
            time *= 1.3;
            eco -= 1.0; // Cutting corners?
        }
        else if (providerName.Contains("Eco") || providerName.Contains("Green"))
        {
            // Sustainable Focus
            cost *= 1.1m; // Slightly more expensive
            eco += 0.5;   // Better rating
        }
    }

    private double CalculateScore(OptimizationPriority priority, OptimizationResult metrics)
    {
        double score = 0;

        switch (priority)
        {
            case OptimizationPriority.LowestCost:
                // Price sensitivity is high. 
                // Formula: Reference Price ($3000) / Actual Price * 10
                score = (3000.0 / (double)metrics.EstimatedCost) * 10;
                break;

            case OptimizationPriority.FastestDelivery:
                // Time sensitivity is high.
                // Formula: Reference Time (20 days) / Actual Time * 10
                score = (20.0 / metrics.EstimatedLeadTimeDays) * 10;
                break;

            case OptimizationPriority.HighestQuality:
            default:
                // Quality/Sustainability focus.
                score = metrics.SustainabilityRating; 
                // Penalty for very cheap options (suspicious quality)
                if (metrics.EstimatedCost < 1000) score -= 2; 
                break;
        }

        return Math.Clamp(score, 0, 10); // Ensure 0-10 range
    }
}