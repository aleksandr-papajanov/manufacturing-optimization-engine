using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Entities;
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

        // Create allocated slot with segments (working time + breaks)
        var allocatedSlotEntity = new AllocatedSlotEntity
        {
            StartTime = evt.AllocatedSlot?.StartTime ?? DateTime.UtcNow,
            EndTime = evt.AllocatedSlot?.EndTime ?? DateTime.UtcNow
        };

        var allocatedSlotModel = new AllocatedSlotModel
        {
            StartTime = allocatedSlotEntity.StartTime,
            EndTime = allocatedSlotEntity.EndTime
        };

        if (evt.AllocatedSlot != null)
        {
            var allSegments = await _providerLogic.GetAllSegmentsAsync(evt.AllocatedSlot);
            
            for (int i = 0; i < allSegments.Count; i++)
            {
                var segmentEntity = new TimeSegmentEntity
                {
                    StartTime = allSegments[i].StartTime,
                    EndTime = allSegments[i].EndTime,
                    SegmentType = allSegments[i].SegmentType.ToString(),
                    SegmentOrder = i
                };
                
                allocatedSlotEntity.Segments.Add(segmentEntity);
                allocatedSlotModel.Segments.Add(allSegments[i]);
            }
        }

        // Create planned process with reference to allocated slot
        var plannedProcessEntity = new PlannedProcessEntity
        {
            ProposalId = proposalEntity.Id,
            AllocatedSlot = allocatedSlotEntity
        };

        proposalEntity.PlannedProcess = plannedProcessEntity;

        await _proposalRepository.UpdateAsync(proposalEntity);
        await _proposalRepository.SaveChangesAsync();

        responseEvent.IsAccepted = true;
        responseEvent.AllocatedSlot = allocatedSlotModel;

        _messagePublisher.PublishReply(evt, responseEvent);
    }
}
