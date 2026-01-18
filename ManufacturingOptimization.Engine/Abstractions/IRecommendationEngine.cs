using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Abstractions;

public interface IRecommendationEngine
{
    /// <summary>
    /// Analyzes available providers and returns a ranked list of recommendations.
    /// </summary>
    List<ProviderRecommendation> GenerateRecommendations(OptimizationRequest request, IEnumerable<Provider> capableProviders);
}