using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using Common.Models;

namespace ManufacturingOptimization.ProviderSimulator;

public class ProviderSimulatorWorker : BackgroundService
{
    private readonly ILogger<ProviderSimulatorWorker> _logger;
    private readonly IMessagingInfrastructure _messagingInfrastructure;
    private readonly IMessageSubscriber _messageSubscriber;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProviderSimulator _providerLogic;

    public ProviderSimulatorWorker(
        ILogger<ProviderSimulatorWorker> logger,
        IMessagingInfrastructure messagingInfrastructure,
        IMessageSubscriber messageSubscriber,
        IMessagePublisher messagePublisher,
        IProviderSimulator providerLogic)
    {
        _logger = logger;
        _messagingInfrastructure = messagingInfrastructure;
        _messageSubscriber = messageSubscriber;
        _messagePublisher = messagePublisher;
        _providerLogic = providerLogic;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        SetupRabbitMq();

        // Publish ServiceReadyEvent after a short delay to ensure subscriptions are ready
        await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        PublishServiceReady();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        // Listen to proposals from Engine
        _messagingInfrastructure.DeclareExchange(Exchanges.Process);
        _messagingInfrastructure.DeclareQueue("provider.process.proposals");
        _messagingInfrastructure.BindQueue("provider.process.proposals", Exchanges.Process, ProcessRoutingKeys.Propose);
        _messagingInfrastructure.PurgeQueue("provider.process.proposals");

        // Listen to estimate requests for this specific provider
        var estimateQueueName = $"process.estimate.{_providerLogic.ProviderId}";
        _messagingInfrastructure.DeclareQueue(estimateQueueName);
        _messagingInfrastructure.BindQueue(estimateQueueName, Exchanges.Process, estimateQueueName);
        _messagingInfrastructure.PurgeQueue(estimateQueueName);
        _messageSubscriber.Subscribe<RequestProcessEstimateCommand>(estimateQueueName, HandleEstimateRequest);

        // Listen to provider coordination commands
        var providerCoordinationQueue = $"provider.coordination.{_providerLogic.ProviderId}";
        _messagingInfrastructure.DeclareQueue(providerCoordinationQueue);
        _messagingInfrastructure.BindQueue(providerCoordinationQueue, Exchanges.Provider, ProviderRoutingKeys.StartAll);
        _messagingInfrastructure.PurgeQueue(providerCoordinationQueue);
        _messageSubscriber.Subscribe<StartAllProvidersCommand>(providerCoordinationQueue, HandleStartAllProviders);

        // Send responses back to Engine (exchange already declared by Engine)
        // Responses go to the same Process exchange

        // Setup for provider registration
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
    }

    private void HandleEstimateRequest(RequestProcessEstimateCommand request)
    {
        var estimate = _providerLogic.HandleEstimateRequest(request);

        _messagePublisher.PublishReply(request.ReplyTo, request.CommandId.ToString(), estimate);
    }
    
    private void PublishServiceReady()
    {
        var queues = new List<string>
        {
            "provider.process.proposals",
            $"process.estimate.{_providerLogic.ProviderId}",
            $"provider.coordination.{_providerLogic.ProviderId}"
        };
        
        var evt = new ServiceReadyEvent
        {
            ServiceName = $"ProviderSimulator_{_providerLogic.ProviderName}",
            SubscribedQueues = queues
        };
        
        _messagePublisher.Publish(Exchanges.System, SystemRoutingKeys.ServiceReady, evt);
    }
    
    private void HandleStartAllProviders(StartAllProvidersCommand command)
    {
        PublishProviderRegistered();
    }

    private void PublishProviderRegistered()
    {
        var registeredEvent = new ProviderRegisteredEvent
        {
            ProviderId = _providerLogic.ProviderId,
            ProviderType = _providerLogic.GetType().Name,
            ProviderName = _providerLogic.ProviderName,
            ProcessCapabilities = _providerLogic.ProcessCapabilities,
            TechnicalCapabilities = _providerLogic.TechnicalCapabilities
        };

        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.Registered, registeredEvent);
    }
}
