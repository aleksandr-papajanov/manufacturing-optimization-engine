using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using System.Collections.Concurrent;

namespace ManufacturingOptimization.Analytics.Services;

public interface IAnalyticsStore
{
    void RecordSelection(SelectStrategyCommand selection);
    AnalyticsSummary GetSummary();
}

public class InMemoryAnalyticsStore : IAnalyticsStore
{
    private readonly ConcurrentBag<SelectStrategyCommand> _history = new();

    public void RecordSelection(SelectStrategyCommand selection)
    {
        _history.Add(selection);
    }

    public AnalyticsSummary GetSummary()
    {
        return new AnalyticsSummary
        {
            TotalDeals = _history.Count,
            
            // New Metric: Total CO2 Saved
            TotalCO2Saved = _history.Sum(CalculateCO2Impact),

            TopProvider = _history
                .GroupBy(x => x.SelectedProviderId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "None"
        };
    }

    // Business Logic: Assign CO2 values based on strategy type
    private int CalculateCO2Impact(SelectStrategyCommand command)
    {
        if (string.IsNullOrEmpty(command.SelectedStrategyName)) return 0;

        var strategy = command.SelectedStrategyName.ToLower();

        if (strategy.Contains("refurbish")) return 500; // High impact
        if (strategy.Contains("rewind")) return 300;    // Medium impact
        if (strategy.Contains("upgrade")) return 100;   // Low impact
        
        return 0; // Replacement usually has high manufacturing cost (0 savings)
    }
}

public class AnalyticsSummary
{
    public int TotalDeals { get; set; }
    public double TotalCO2Saved { get; set; } // New Field
    public string TopProvider { get; set; } = string.Empty;
}