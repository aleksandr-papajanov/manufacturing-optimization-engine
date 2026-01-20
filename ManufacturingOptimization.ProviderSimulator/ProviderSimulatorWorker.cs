using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagement;
using ManufacturingOptimization.Common.Messaging.Messages.SystemManagement;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

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

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        // Listen to proposals from Engine
        _messagingInfrastructure.DeclareExchange(Exchanges.Process);
        
        // Listen to process proposals for this specific provider
        var proposalQueueName = $"process.proposal.{_providerLogic.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(proposalQueueName);
        _messagingInfrastructure.BindQueue(proposalQueueName, Exchanges.Process, proposalQueueName);
        _messagingInfrastructure.PurgeQueue(proposalQueueName);
        _messageSubscriber.Subscribe<ProposeProcessToProviderCommand>(proposalQueueName, HandleProposal);

        // Listen to confirmations for this specific provider
        var confirmationQueueName = $"process.confirm.{_providerLogic.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(confirmationQueueName);
        _messagingInfrastructure.BindQueue(confirmationQueueName, Exchanges.Process, confirmationQueueName);
        _messagingInfrastructure.PurgeQueue(confirmationQueueName);
        _messageSubscriber.Subscribe<ConfirmProcessProposalCommand>(confirmationQueueName, HandleConfirmation);

        // Listen to provider coordination commands
        var providerCoordinationQueue = $"provider.coordination.{_providerLogic.Provider.Id}";
        _messagingInfrastructure.DeclareQueue(providerCoordinationQueue);
        _messagingInfrastructure.BindQueue(providerCoordinationQueue, Exchanges.Provider, ProviderRoutingKeys.RequestRegistrationAll);
        _messagingInfrastructure.PurgeQueue(providerCoordinationQueue);
        _messageSubscriber.Subscribe<RequestProvidersRegistrationCommand>(providerCoordinationQueue, HandleProvidersRegistrationRequest);

        // Setup for provider registration
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
    }

    private void HandleProposal(ProposeProcessToProviderCommand proposal)
    {
        var response = _providerLogic.HandleProposal(proposal);
        _messagePublisher.PublishReply(proposal, response);
    }

    private void HandleConfirmation(ConfirmProcessProposalCommand confirmation)
    {
        var response = _providerLogic.HandleConfirmation(confirmation);
        _messagePublisher.PublishReply(confirmation, response);
    }
    
    private void HandleProvidersRegistrationRequest(RequestProvidersRegistrationCommand command)
    {
        var registeredEvent = new ProviderRegisteredEvent
        {
            Provider = _providerLogic.Provider
        };

        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.Registered, registeredEvent);
    }
}
