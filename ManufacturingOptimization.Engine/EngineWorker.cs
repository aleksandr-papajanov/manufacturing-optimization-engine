using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.Engine.Abstractions;

namespace ManufacturingOptimization.Engine;

public class EngineWorker : BackgroundService
{
    private readonly ILogger<EngineWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProviderRepository _providerRegistry;

    public EngineWorker(
        ILogger<EngineWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IProviderRepository providerRegistry)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _providerRegistry = providerRegistry;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetupMessaging();

        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.StartAll, new StartAllProvidersCommand());

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupMessaging()
    {
        // Provider events
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        _messagingInfrastructure.DeclareQueue("engine.provider.events");
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.BindQueue("engine.provider.events", Exchanges.Provider, ProviderRoutingKeys.AllReady);
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("engine.provider.events", HandleProviderRegistered);
        _messageSubscriber.Subscribe<AllProvidersReadyEvent>("engine.provider.events", HandleProvidersReady);

        // Optimization requests
        _messagingInfrastructure.DeclareExchange(Exchanges.Optimization);
        _messagingInfrastructure.DeclareQueue("engine.optimization.requests");
        _messagingInfrastructure.BindQueue("engine.optimization.requests", Exchanges.Optimization, "optimization.request");
        _messageSubscriber.Subscribe<RequestOptimizationPlanCommand>("engine.optimization.requests", HandleOptimizationRequest);

        // Process events
        _messagingInfrastructure.DeclareExchange(Exchanges.Process);
        _messagingInfrastructure.DeclareQueue("engine.process.responses");
        _messagingInfrastructure.BindQueue("engine.process.responses", Exchanges.Process, ProcessRoutingKeys.Accepted);
        _messagingInfrastructure.BindQueue("engine.process.responses", Exchanges.Process, ProcessRoutingKeys.Declined);
        _messageSubscriber.Subscribe<ProcessAcceptedEvent>("engine.process.responses", HandleProcessAccepted);
        _messageSubscriber.Subscribe<ProcessDeclinedEvent>("engine.process.responses", HandleProcessDeclined);
    }

    private void HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        _providerRegistry.Create(evt.ProviderId, evt.ProviderType, evt.ProviderName);
        _logger.LogInformation("Provider registered: {ProviderName} ({ProviderId})", evt.ProviderName, evt.ProviderId);
    }

    private void HandleProvidersReady(AllProvidersReadyEvent readyEvent)
    {
        _logger.LogInformation("All {Count} providers are ready", _providerRegistry.Count);
    }

    private void HandleOptimizationRequest(RequestOptimizationPlanCommand command)
    {
        var providers = _providerRegistry.GetAll().ToList();
        
        if (providers.Count == 0)
        {
            _logger.LogWarning("No providers available");
            return;
        }

        var random = new Random();
        var selectedProvider = providers[random.Next(providers.Count)];
        
        _logger.LogInformation("Sending request to {ProviderName}", selectedProvider.ProviderName);

        var proposal = new ProposeProcessCommand 
        { 
            CommandId = command.CommandId,
            ProviderId = selectedProvider.ProviderId
        };
        
        _messagePublisher.Publish(Exchanges.Process, ProcessRoutingKeys.Propose, proposal);
    }

    private void HandleProcessAccepted(ProcessAcceptedEvent evt)
    {
        _logger.LogInformation("Provider {ProviderId} accepted", evt.ProviderId);
        
        var planEvent = new OptimizationPlanCreatedEvent 
        { 
            CommandId = evt.CommandId,
            ProviderId = evt.ProviderId,
            Response = "accepted"
        };
        
        _messagePublisher.Publish(Exchanges.Optimization, "optimization.response", planEvent);
    }

    private void HandleProcessDeclined(ProcessDeclinedEvent evt)
    {
        _logger.LogInformation("Provider {ProviderId} declined", evt.ProviderId);
        
        var planEvent = new OptimizationPlanCreatedEvent 
        { 
            CommandId = evt.CommandId,
            ProviderId = evt.ProviderId,
            Response = "declined"
        };
        
        _messagePublisher.Publish(Exchanges.Optimization, "optimization.response", planEvent);
    }
}

