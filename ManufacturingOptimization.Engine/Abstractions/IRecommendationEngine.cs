using Common.Models; // Correct namespace found in your file
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Abstractions;

public interface IRecommendationEngine
{
    /// <summary>
    /// Analyzes available providers and returns a ranked list of recommendations.
    /// </summary>
    List<ProviderRecommendation> GenerateRecommendations(MotorRequest request, IEnumerable<Provider> capableProviders);
}