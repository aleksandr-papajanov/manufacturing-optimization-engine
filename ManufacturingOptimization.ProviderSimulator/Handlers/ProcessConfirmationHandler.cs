using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

/// <summary>
/// Handles process confirmation commands from the Engine.
/// </summary>
public class ProcessConfirmationHandler : IMessageHandler<ConfirmProcessProposalCommand>
{
    private readonly IProviderSimulator _providerLogic;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProposalRepository _proposalRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<ProcessConfirmationHandler> _logger;

    public ProcessConfirmationHandler(
        IProviderSimulator providerLogic,
        IMessagePublisher messagePublisher,
        IProposalRepository proposalRepository,
        IMapper mapper,
        ILogger<ProcessConfirmationHandler> logger)
    {
        _providerLogic = providerLogic;
        _messagePublisher = messagePublisher;
        _proposalRepository = proposalRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task HandleAsync(ConfirmProcessProposalCommand evt)
    {
        var responseEvent = new ProcessProposalReviewedEvent
        {
            ProposalId = evt.ProposalId,
        };

        var proposalEntity = await _proposalRepository.GetByIdAsync(evt.ProposalId);

        if (proposalEntity == null)
        {
            responseEvent.IsAccepted = false;
            responseEvent.DeclineReason = "Proposal not found.";
            _messagePublisher.PublishReply(evt, responseEvent);
            return;
        }

        _providerLogic.HandleConfirmation(proposalEntity);

        proposalEntity.Status = ProposalStatus.Confirmed;
        proposalEntity.ModifiedAt = DateTime.UtcNow;
        await _proposalRepository.UpdateAsync(proposalEntity);
        await _proposalRepository.SaveChangesAsync();

        var plannedProcessEntity = new PlannedProcessEntity
        {
            ProposalId = proposalEntity.Id
        };

        proposalEntity.PlannedProcess = plannedProcessEntity;

        await _proposalRepository.UpdateAsync(proposalEntity);
        await _proposalRepository.SaveChangesAsync();

        responseEvent.IsAccepted = true;

        _messagePublisher.PublishReply(evt, responseEvent);
    }
}
