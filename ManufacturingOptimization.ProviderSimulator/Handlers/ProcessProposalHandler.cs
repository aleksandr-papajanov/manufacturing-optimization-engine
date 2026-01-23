using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

/// <summary>
/// Handles process proposals from the Engine by evaluating feasibility and responding.
/// </summary>
public class ProcessProposalHandler : IMessageHandler<ProposeProcessToProviderCommand>
{
    private readonly IProviderSimulator _providerLogic;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<ProcessProposalHandler> _logger;

    public ProcessProposalHandler(
        IProviderSimulator providerLogic,
        IMessagePublisher messagePublisher,
        ILogger<ProcessProposalHandler> logger)
    {
        _providerLogic = providerLogic;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public Task HandleAsync(ProposeProcessToProviderCommand proposal)
    {
        var response = _providerLogic.HandleProposal(proposal);
        _messagePublisher.PublishReply(proposal, response);
        
        return Task.CompletedTask;
    }
}
