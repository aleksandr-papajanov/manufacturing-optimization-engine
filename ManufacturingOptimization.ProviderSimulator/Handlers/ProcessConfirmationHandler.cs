using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.ProviderSimulator.Abstractions;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

/// <summary>
/// Handles process confirmation commands from the Engine.
/// </summary>
public class ProcessConfirmationHandler : IMessageHandler<ConfirmProcessProposalCommand>
{
    private readonly IProviderSimulator _providerLogic;
    private readonly IMessagePublisher _messagePublisher;
    private readonly ILogger<ProcessConfirmationHandler> _logger;

    public ProcessConfirmationHandler(
        IProviderSimulator providerLogic,
        IMessagePublisher messagePublisher,
        ILogger<ProcessConfirmationHandler> logger)
    {
        _providerLogic = providerLogic;
        _messagePublisher = messagePublisher;
        _logger = logger;
    }

    public Task HandleAsync(ConfirmProcessProposalCommand confirmation)
    {
        var response = _providerLogic.HandleConfirmation(confirmation);
        _messagePublisher.PublishReply(confirmation, response);
        
        return Task.CompletedTask;
    }
}
