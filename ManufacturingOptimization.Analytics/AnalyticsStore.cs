using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using System.Collections.Concurrent;

namespace ManufacturingOptimization.Analytics.Services;

public interface IAnalyticsStore
{
    void RecordSelection(SelectStrategyCommand selection);
    AnalyticsSummary GetSummary();
    IEnumerable<SelectStrategyCommand> GetAll(); // <--- New Method
}

public class InMemoryAnalyticsStore : IAnalyticsStore
{
    private readonly ConcurrentBag<SelectStrategyCommand> _history = new();

    public void RecordSelection(SelectStrategyCommand selection)
    {
        _history.Add(selection);
    }

    public IEnumerable<SelectStrategyCommand> GetAll()
    {
        return _history; // <--- Return the raw list
    }

    public AnalyticsSummary GetSummary()
    {
        return new AnalyticsSummary
        {
            TotalDeals = _history.Count,
            TotalCO2Saved = _history.Sum(CalculateCO2Impact),
            TopProvider = _history
                .GroupBy(x => x.SelectedProviderId)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .FirstOrDefault() ?? "None"
        };
    }

    private int CalculateCO2Impact(SelectStrategyCommand command)
    {
        if (string.IsNullOrEmpty(command.SelectedStrategyName)) return 0;
        var strategy = command.SelectedStrategyName.ToLower();
        if (strategy.Contains("refurbish")) return 500;
        if (strategy.Contains("rewind")) return 300;
        if (strategy.Contains("upgrade")) return 100;
        return 0;
    }
}

public class AnalyticsSummary
{
    public int TotalDeals { get; set; }
    public double TotalCO2Saved { get; set; }
    public string TopProvider { get; set; } = string.Empty;
}