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
    // Thread-safe collection to hold the history in memory
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
            TopProvider = _history
                .GroupBy(x => x.SelectedProviderId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "None"
        };
    }
}

public class AnalyticsSummary
{
    public int TotalDeals { get; set; }
    public string TopProvider { get; set; } = string.Empty;
}
