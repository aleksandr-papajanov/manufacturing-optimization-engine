using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.ProviderSimulator.Abstractions;
using ManufacturingOptimization.ProviderSimulator.Data.Entities;
using ManufacturingOptimization.Common.Models.Contracts;
using AutoMapper;

namespace ManufacturingOptimization.ProviderSimulator.Handlers;

/// <summary>
/// Handles process proposals from the Engine by evaluating feasibility and responding.
/// </summary>
public class ProcessProposalHandler : IMessageHandler<ProposeProcessToProviderCommand>
{
    private readonly IProviderSimulator _providerLogic;
    private readonly IMessagePublisher _messagePublisher;
    private readonly IProposalRepository _proposalRepository;
    private readonly ILogger<ProcessProposalHandler> _logger;
    private readonly IMapper _mapper;

    public ProcessProposalHandler(
        IProviderSimulator providerLogic,
        IMessagePublisher messagePublisher,
        IProposalRepository proposalRepository,
        ILogger<ProcessProposalHandler> logger,
        IMapper mapper)
    {
        _providerLogic = providerLogic;
        _messagePublisher = messagePublisher;
        _proposalRepository = proposalRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task HandleAsync(ProposeProcessToProviderCommand proposal)
    {
        var proposalModel = await _providerLogic.HandleProposalAsync(proposal);
        
        var proposalEntity = _mapper.Map<ProposalEntity>(proposalModel);
        
        await _proposalRepository.AddAsync(proposalEntity);
        await _proposalRepository.SaveChangesAsync();

        proposalModel = _mapper.Map<ProposalModel>(proposalEntity);

        var responseEvent = new ProcessProposalEstimatedEvent
        {
            Proposal = proposalModel
        };
        
        _messagePublisher.PublishReply(proposal, responseEvent);
    }
}
