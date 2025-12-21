using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.PlanManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.Gateway.Abstractions;

namespace ManufacturingOptimization.Gateway.Services;

public class GatewayWorker : BackgroundService
{
    private readonly ILogger<GatewayWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IRequestResponseRepository _repository;
    private readonly IProviderRepository _providerRepository;

    public GatewayWorker(
        ILogger<GatewayWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IRequestResponseRepository repository,
        IProviderRepository providerRegistry)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _repository = repository;
        _providerRepository = providerRegistry;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetupRabbitMq();
        SubscribeToResponses();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        // Listen to provider registrations
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
        _messagingInfrastructure.DeclareQueue("gateway.provider.events");
        _messagingInfrastructure.BindQueue("gateway.provider.events", Exchanges.Provider, ProviderRoutingKeys.Registered);

        // Listen to optimization responses
        _messagingInfrastructure.DeclareExchange(Exchanges.Optimization);
        _messagingInfrastructure.DeclareQueue("gateway.optimization.responses");
        _messagingInfrastructure.BindQueue("gateway.optimization.responses", Exchanges.Optimization, "optimization.response");
    }

    private void SubscribeToResponses()
    {
        _messageSubscriber.Subscribe<ProviderRegisteredEvent>("gateway.provider.events", HandleProviderRegistered);
        _messageSubscriber.Subscribe<OptimizationPlanCreatedEvent>("gateway.optimization.responses", HandleOptimizationResponse);
    }

    private void HandleProviderRegistered(ProviderRegisteredEvent evt)
    {
        _providerRepository.Create(evt.ProviderId, evt.ProviderType, evt.ProviderName);
    }

    private void HandleOptimizationResponse(OptimizationPlanCreatedEvent response)
    {
        _repository.AddResponse(response);
    }
}