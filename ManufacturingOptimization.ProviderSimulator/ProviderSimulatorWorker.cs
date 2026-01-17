using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagment;
using ManufacturingOptimization.Common.Messaging.Messages.ProviderManagment;
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
        SubscribeToProposals();
        PublishProviderRegistered();

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private void SetupRabbitMq()
    {
        // Listen to proposals from Engine
        _messagingInfrastructure.DeclareExchange(Exchanges.Process);
        _messagingInfrastructure.DeclareQueue("provider.process.proposals");
        _messagingInfrastructure.BindQueue("provider.process.proposals", Exchanges.Process, ProcessRoutingKeys.Propose);

        // Send responses back to Engine (exchange already declared by Engine)
        // Responses go to the same Process exchange
        
        // Setup for provider registration
        _messagingInfrastructure.DeclareExchange(Exchanges.Provider);
    }

    private void PublishProviderRegistered()
    {
        var registeredEvent = new ProviderRegisteredEvent
        {
            ProviderId = _providerLogic.ProviderId,
            ProviderType = _providerLogic.GetType().Name,
            ProviderName = _providerLogic.ProviderName,
            Capabilities = _providerLogic.Capabilities,
            TechnicalCapabilities = new TechnicalCapabilities
            {
                AxisHeight = _providerLogic.AxisHeight,
                Power = _providerLogic.Power,
                Tolerance = _providerLogic.Tolerance
            }
        };

        _messagePublisher.Publish(Exchanges.Provider, ProviderRoutingKeys.Registered, registeredEvent);
    }

    private void SubscribeToProposals()
    {
        _messageSubscriber.Subscribe<ProposeProcessCommand>("provider.process.proposals", HandleProcessProposal);
    }

    private void HandleProcessProposal(ProposeProcessCommand proposal)
    {
        var accepted = _providerLogic.HandleProposal(proposal);
        
        if (accepted)
        {
            var acceptEvent = new ProcessAcceptedEvent
            {
                CommandId = proposal.CommandId,
                ProviderId = _providerLogic.ProviderId,
            };
            
            _messagePublisher.Publish(Exchanges.Process, ProcessRoutingKeys.Accepted, acceptEvent);
        }
        else
        {
            var declineEvent = new ProcessDeclinedEvent
            {
                CommandId = proposal.CommandId,
                ProviderId = _providerLogic.ProviderId,
            };
            
            _messagePublisher.Publish(Exchanges.Process, ProcessRoutingKeys.Declined, declineEvent);
        }
    }
}
