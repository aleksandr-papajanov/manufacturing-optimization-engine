using ManufacturingOptimization.Analytics.Services;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;

namespace ManufacturingOptimization.Analytics;

public class AnalyticsWorker : BackgroundService
{
    private readonly ILogger<AnalyticsWorker> _logger;
    private readonly IMessageSubscriber _subscriber;
    private readonly IMessagingInfrastructure _infra; // <--- 1. Add this
    private readonly IAnalyticsStore _store;

    public AnalyticsWorker(
        ILogger<AnalyticsWorker> logger, 
        IMessageSubscriber subscriber,
        IMessagingInfrastructure infra, // <--- 2. Inject this
        IAnalyticsStore store)
    {
        _logger = logger;
        _subscriber = subscriber;
        _infra = infra; // <--- 3. Assign this
        _store = store;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Analytics Worker started. Waiting for data...");

        // Subscribe to the queue
        _subscriber.Subscribe<SelectStrategyCommand>("analytics.strategy.selected", HandleStrategySelected);

        // --- 4. ADD THIS BINDING ---
        // This connects your queue to the "optimization.exchange" so you get the messages!
        _infra.BindQueue("analytics.strategy.selected", "optimization.exchange", "optimization.strategy.selected");
        // ----------------------------

        return Task.CompletedTask;
    }

    private void HandleStrategySelected(SelectStrategyCommand message)
    {
        _logger.LogInformation("--- 📊 NEW DATA POINT RECEIVED ---");
        
        _store.RecordSelection(message);
        
        var summary = _store.GetSummary();
        _logger.LogInformation($"Total Deals: {summary.TotalDeals} | Top Provider: {summary.TopProvider}");
    }
}