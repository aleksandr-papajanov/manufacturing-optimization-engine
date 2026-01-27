using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Proposes processes to matched providers for preliminary acceptance.
/// Providers can accept with estimates or decline the proposal.
/// This step supports both proposal-based and direct estimation flows.
/// </summary>
public class EstimationStep : IWorkflowStep
{
    private readonly IMessagePublisher _messagePublisher;

    public EstimationStep(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public string Name => "Proposal & Estimation";

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        foreach (var step in context.ProcessSteps)
        {
            var proposalTasks = step.MatchedProviders.Select(async provider =>
            {
                try
                {
                    var proposal = new ProposeProcessToProviderCommand
                    {
                        RequestId = context.Request.RequestId,
                        ProviderId = provider.ProviderId,
                        Process = step.Process,
                        MotorSpecs = context.Request.MotorSpecs
                    };

                    var response = await _messagePublisher.RequestReplyAsync<ProcessProposalEstimatedEvent>(
                        Exchanges.Process,
                        $"process.proposal.{provider.ProviderId}",
                        proposal,
                        TimeSpan.FromMinutes(10));

                    if (response != null)
                    {
                        switch (response.Proposal.Status)
                        {
                            case ProposalStatus.Accepted:
                                provider.Estimate = response.Proposal.Estimate
                                    ?? throw new InvalidOperationException("Accepted proposal must include an estimate");
                                break;
                            case ProposalStatus.Declined:
                                // Nothing to do, move on
                                break;
                            default:
                                throw new InvalidOperationException($"Unexpected proposal status: {response.Proposal.Status}");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"No response received from provider");
                    }
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Provider {provider.ProviderId} estimation failed for {step.Process}: {ex.Message}", ex);
                }
            });

            await Task.WhenAll(proposalTasks);
        }
    }
}
