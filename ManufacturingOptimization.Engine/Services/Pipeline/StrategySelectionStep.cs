using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.PanManagement;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Publishes generated strategies and waits for customer selection.
/// This step blocks until customer selects a strategy via SelectStrategyCommand.
/// </summary>
public class StrategySelectionStep : IWorkflowStep
{
    private readonly TimeSpan TIMEOUT = TimeSpan.FromMinutes(10);
    private readonly ILogger<StrategySelectionStep> _logger;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;

    public string Name => "Strategy Selection";

    public StrategySelectionStep(
        ILogger<StrategySelectionStep> logger,
        IMessagePublisher messagePublisher,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber)
    {
        _logger = logger;
        _messagePublisher = messagePublisher;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (context.Strategies.Count == 0)
        {
            context.Errors.Add("No strategies available for selection");
            return;
        }

        var requestId = context.Request.RequestId;

        // Create dedicated queue for this request's selection
        var selectionQueueName = $"engine.strategy.selection.{requestId}";
        var selectionRoutingKey = $"{OptimizationRoutingKeys.StrategySelected}.{requestId}";
        
        // TaskCompletionSource to wait for selection
        var selectionTcs = new TaskCompletionSource<SelectStrategyCommand>();

        try
        {
            // Setup temporary queue for this specific request
            _messagingInfrastructure.DeclareQueue(selectionQueueName);
            _messagingInfrastructure.BindQueue(selectionQueueName, Exchanges.Optimization, selectionRoutingKey);

            // Subscribe to selection for THIS request only
            _messageSubscriber.Subscribe<SelectStrategyCommand>(
                selectionQueueName,
                command =>
                {
                    selectionTcs.TrySetResult(command);
                });

            // Publish strategies to customer
            var strategiesEvent = new MultipleStrategiesReadyEvent
            {
                CommandId = Guid.NewGuid(),
                RequestId = requestId,
                WorkflowType = context.WorkflowType ?? "Unknown",
                Strategies = context.Strategies,
                IsSuccess = true
            };

            _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.StrategiesReady, strategiesEvent);

            // Wait for customer selection (with timeout)
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TIMEOUT);

            try
            {
                var selectionCommand = await selectionTcs.Task.WaitAsync(cts.Token);
                
                // Find the selected strategy
                var selectedStrategy = context.Strategies.FirstOrDefault(s => s.StrategyId == selectionCommand.SelectedStrategyId);

                if (selectedStrategy == null)
                {
                    context.Errors.Add($"Selected strategy not found: {selectionCommand.SelectedStrategyId}");
                    return;
                }

                context.SelectedStrategy = selectedStrategy;
            }
            catch (OperationCanceledException) when (cts.Token.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                // Timeout occurred
                context.Errors.Add("Customer selection timeout - no strategy selected within 10 minutes");
            }
        }
        finally
        {
            // Cleanup: purge temporary queue
            try
            {
                _messagingInfrastructure.PurgeQueue(selectionQueueName);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup selection queue: {QueueName}", selectionQueueName);
            }
        }
    }
}

