using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Gateway.Abstractions;
using System.Collections.Concurrent;

namespace ManufacturingOptimization.Gateway.Services;

/// <summary>
/// In-memory cache for strategies awaiting customer selection.
/// Stores strategies temporarily until customer makes selection or timeout occurs.
/// </summary>
public class InMemoryOptimizationStrategyRepository : IOptimizationStrategyRepository
{
    private readonly ConcurrentDictionary<Guid, List<OptimizationStrategy>> _strategies = new();
    private readonly ConcurrentDictionary<Guid, DateTime> _timestamps = new();
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Store strategies for a request.
    /// </summary>
    public void StoreStrategies(Guid requestId, List<OptimizationStrategy> strategies)
    {
        _strategies[requestId] = strategies;
        _timestamps[requestId] = DateTime.UtcNow;
    }

    /// <summary>
    /// Retrieve strategies for a request.
    /// </summary>
    public List<OptimizationStrategy>? GetStrategies(Guid requestId)
    {
        CleanupExpired();

        if (_strategies.TryGetValue(requestId, out var strategies))
        {
            return strategies;
        }

        return null;
    }

    /// <summary>
    /// Remove strategies after selection.
    /// </summary>
    public void RemoveStrategies(Guid requestId)
    {
        _strategies.TryRemove(requestId, out _);
        _timestamps.TryRemove(requestId, out _);
    }

    /// <summary>
    /// Clean up expired cache entries.
    /// </summary>
    private void CleanupExpired()
    {
        var now = DateTime.UtcNow;
        var expiredKeys = _timestamps
            .Where(kvp => now - kvp.Value > _cacheExpiration)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _strategies.TryRemove(key, out _);
            _timestamps.TryRemove(key, out _);
        }
    }
}
