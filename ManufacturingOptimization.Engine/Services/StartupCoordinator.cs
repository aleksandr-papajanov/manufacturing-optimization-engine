using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;

namespace ManufacturingOptimization.Engine.Services;

/// <summary>
/// Coordinates system startup by waiting for all services to report readiness
/// before allowing the Engine to start processing requests.
/// </summary>
public class StartupCoordinator : BackgroundService
{
    private readonly ILogger<StartupCoordinator> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    
    private readonly HashSet<string> _readyServices = new();
    private readonly List<string> _requiredServices = new() 
    { 
        "Gateway", 
        "ProviderRegistry", 
        "Engine"
    };
    private bool _systemReadyPublished = false;

    public StartupCoordinator(
        ILogger<StartupCoordinator> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;

        SetupRabbitMq();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        _messagingInfrastructure.DeclareExchange(Exchanges.System);
        
        _messagingInfrastructure.DeclareQueue("coordinator.service.ready");
        _messagingInfrastructure.BindQueue("coordinator.service.ready", Exchanges.System, SystemRoutingKeys.ServiceReady);
        _messagingInfrastructure.PurgeQueue("coordinator.service.ready");
        
        _messageSubscriber.Subscribe<ServiceReadyEvent>("coordinator.service.ready", HandleServiceReady);
    }

    private void HandleServiceReady(ServiceReadyEvent evt)
    {
        lock (_readyServices)
        {
            if (_readyServices.Add(evt.ServiceName))
            {
                CheckAndPublishSystemReady();
            }
        }
    }

    private void CheckAndPublishSystemReady()
    {
        if (_systemReadyPublished)
            return;
            
        var allReady = _requiredServices.All(s => _readyServices.Contains(s));
        
        if (allReady)
        {
            var evt = new SystemReadyEvent
            {
                ReadyServices = _readyServices.ToList()
            };
            
            _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.SystemReady, evt);
            _systemReadyPublished = true;
        }
        else
        {
            var missing = _requiredServices.Except(_readyServices).ToList();
        }
    }
}
