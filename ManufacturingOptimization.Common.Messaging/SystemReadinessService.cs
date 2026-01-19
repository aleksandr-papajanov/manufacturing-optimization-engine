using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ManufacturingOptimization.Common.Messaging;

/// <summary>
/// Base class for system readiness coordination.
/// - Implements ISystemReadinessService with TaskCompletionSource for blocking operations
/// - Listens for SystemReadyEvent and marks service ready
/// - Can be extended for additional startup logic (see StartupCoordinator in Engine)
/// </summary>
public class SystemReadinessService : BackgroundService, ISystemReadinessService
{
    private readonly TaskCompletionSource<bool> _systemReadyTcs = new();
    protected readonly ILogger _logger;
    protected readonly IMessagingInfrastructure _messagingInfrastructure;
    protected readonly IMessageSubscriber _messageSubscriber;
    private readonly string _serviceName;
    private readonly string _queueName;

    public SystemReadinessService(
        ILogger<SystemReadinessService> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IOptions<SystemReadinessSettings> settings)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _serviceName = settings.Value.ServiceName;
        _queueName = $"{_serviceName.ToLower()}.system.ready";

        SetupRabbitMq();
    }

    public bool IsSystemReady => _systemReadyTcs.Task.IsCompleted;

    public async Task WaitForSystemReadyAsync(CancellationToken cancellationToken = default)
    {
        if (IsSystemReady)
            return;
        
        try
        {
            await _systemReadyTcs.Task.WaitAsync(cancellationToken);
            _logger.LogInformation("System is ready!");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("System readiness wait was cancelled");
        }
    }

    public void MarkSystemReady()
    {
        if (!_systemReadyTcs.Task.IsCompleted)
        {
            _systemReadyTcs.TrySetResult(true);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    protected virtual void SetupRabbitMq()
    {
        // Listen for SystemReadyEvent to mark local service ready
        _messagingInfrastructure.DeclareQueue(_queueName);
        _messagingInfrastructure.BindQueue(_queueName, Exchanges.System, SystemRoutingKeys.SystemReady);
        _messagingInfrastructure.PurgeQueue(_queueName);
        _messageSubscriber.Subscribe<SystemReadyEvent>(_queueName, HandleSystemReady);
    }

    protected virtual void HandleSystemReady(SystemReadyEvent evt)
    {
        MarkSystemReady();
    }
}
