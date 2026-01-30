using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;

namespace ManufacturingOptimization.ProviderRegistry.Services;

public class ExecuteProcessCommandHandler : IMessageHandler<ExecuteProcessCommand>
{
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<ExecuteProcessCommandHandler> _logger;

    public ExecuteProcessCommandHandler(IMessagePublisher publisher, ILogger<ExecuteProcessCommandHandler> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task HandleAsync(ExecuteProcessCommand message)
    {
        _logger.LogInformation("🏭 Provider {ProviderId}: Received order to execute '{Process}' (Plan {PlanId})", 
            message.TargetProviderId, message.ProcessName, message.PlanId);

        // 1. Simulate Manufacturing Work (Delay)
        await Task.Delay(5000);

        _logger.LogInformation("✅ Provider {ProviderId}: Finished '{Process}'. Sending completion event.", 
            message.TargetProviderId, message.ProcessName);

        // 2. Send Completion Event
        var completionEvent = new ProcessExecutionCompletedEvent
        {
            PlanId = message.PlanId,
            StepId = message.StepId,
            ProviderId = message.TargetProviderId,
            Success = true,
            FailureReason = string.Empty
        };

        await _publisher.PublishAsync(Exchanges.Process, "process.execution.completed", completionEvent);
    }
}