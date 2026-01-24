using ManufacturingOptimization.Common.Messaging.Abstractions;
// FIX: Use the existing namespace
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment; 

namespace ManufacturingOptimization.Analytics;

public class AnalyticsWorker : BackgroundService
{
    private readonly ILogger<AnalyticsWorker> _logger;
    private readonly IMessageSubscriber _subscriber;

    public AnalyticsWorker(ILogger<AnalyticsWorker> logger, IMessageSubscriber subscriber)
    {
        _logger = logger;
        _subscriber = subscriber;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Analytics Worker started. Waiting for data...");

        _subscriber.Subscribe<SelectStrategyCommand>("analytics.strategy.selected", HandleStrategySelected);

        return Task.CompletedTask;
    }

    private void HandleStrategySelected(SelectStrategyCommand message)
    {
        _logger.LogInformation("--- 📊 NEW DATA POINT RECEIVED ---");
        _logger.LogInformation($"Request ID:   {message.RequestId}");
        // Note: The existing class might use 'SelectedProviderId' instead of 'ProviderId'
        // Let's check the property names in the next step if this fails.
        _logger.LogInformation($"Winner:       {message.SelectedProviderId}"); 
        _logger.LogInformation($"Strategy:     {message.SelectedStrategyName}");
        _logger.LogInformation("----------------------------------");
    }
}