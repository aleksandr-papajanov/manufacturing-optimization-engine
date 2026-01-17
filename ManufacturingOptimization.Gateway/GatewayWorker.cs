using Common.Models;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.Gateway.Abstractions;

namespace ManufacturingOptimization.Gateway.Services;

public class GatewayWorker : BackgroundService
{
    private readonly ILogger<GatewayWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IRequestResponseRepository _repository;
    private readonly IProviderRepository _providerRepository;

    public GatewayWorker(
        ILogger<GatewayWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IRequestResponseRepository repository,
        IProviderRepository providerRegistry)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _repository = repository;
        _providerRepository = providerRegistry;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetupRabbitMq();
        
        // Give subscriptions time to register
        await Task.Delay(1000, stoppingToken);
        
        // Publish service ready event
        var readyEvent = new ServiceReadyEvent
        {
            ServiceName = "Gateway",
            SubscribedQueues = new List<string> { "gateway.provider.events", "gateway.optimization.responses" }
        };

        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, readyEvent);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        // Listen to provider registrations
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        _messagingInfrastructure.DeclareQueue("gateway.provider.events");
        _messagingInfrastructure.BindQueue("gateway.provider.events", Exchanges.Provider, ProviderRoutingKeys.Registered);
        _messagingInfrastructure.PurgeQueue("gateway.provider.events");

        // Listen to optimization responses
        _messagingInfrastructure.DeclareExchange(Exchanges.Optimization);
        _messagingInfrastructure.DeclareQueue("gateway.optimization.responses");
        _messagingInfrastructure.BindQueue("gateway.optimization.responses", Exchanges.Optimization, "optimization.response");
        _messagingInfrastructure.PurgeQueue("gateway.optimization.responses");

        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("gateway.provider.events", HandleProviderRegistered);
        _messageSubscriber.Subscribe<OptimizationPlanCreatedEvent>("gateway.optimization.responses", HandleOptimizationResponse);
    }

    private void HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        var provider = new Provider
        {
            Id = evt.ProviderId,
            Type = evt.ProviderType,
            Name = evt.ProviderName,
            Enabled = true,
            ProcessCapabilities = evt.ProcessCapabilities,
            TechnicalCapabilities = evt.TechnicalCapabilities
        };
        
        _providerRepository.Create(provider);
    }

    private void HandleOptimizationResponse(OptimizationPlanCreatedEvent response)
    {
        _repository.AddResponse(response);
    }
}