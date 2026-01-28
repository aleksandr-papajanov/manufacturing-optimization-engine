using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Exceptions;
using ManufacturingOptimization.Engine.Models;
using ManufacturingOptimization.Engine.Models.OptimizationStep;

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
        context.Plan.Status = OptimizationPlanStatus.EstimatingCosts;
        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanUpdated, new OptimizationPlanUpdatedEvent
        {
            Plan = context.Plan
        });

        var errors = new List<string>();
        
        foreach (var step in context.ProcessSteps)
        {
            var proposalTasks = step.MatchedProviders.Select(provider => ProposeToProviderAsync(context, step, provider, errors));

            await Task.WhenAll(proposalTasks);
        }
        
        if (errors.Any())
            throw new OptimizationException($"Estimation failed: {string.Join("; ", errors)}");
    }

    private async Task ProposeToProviderAsync(WorkflowContext context, WorkflowProcessStep step, MatchedProvider provider, List<string> errors)
    {
        try
        {
            var proposal = new ProposeProcessToProviderCommand
            {
                RequestId = context.Request.RequestId,
                ProviderId = provider.ProviderId,
                Process = step.Process,
                MotorSpecs = context.Request.MotorSpecs,
                RequestedTimeWindow = context.Request.Constraints.TimeWindow
            };

            var response = await _messagePublisher.RequestReplyAsync<ProcessProposalEstimatedEvent>(
                Exchanges.Process,
                $"process.proposal.{provider.ProviderId}",
                proposal,
                TimeSpan.FromMinutes(10));

            if (response == null)
                throw new OptimizationException($"Provider {provider.ProviderName} did not respond to process proposal within timeout");

            switch (response.Proposal.Status)
            {
                case ProposalStatus.Accepted:
                    provider.Estimate = response.Proposal.Estimate
                        ?? throw new OptimizationException($"Provider {provider.ProviderName} accepted proposal but did not provide an estimate");
                    break;
                case ProposalStatus.Declined:
                    // Nothing to do, move on
                    break;
                default:
                    throw new OptimizationException($"Provider {provider.ProviderName} returned unexpected proposal status: {response.Proposal.Status}");
            }
        }
        catch (OptimizationException ex)
        {
            errors.Add(ex.Message);
        }
        catch (Exception ex)
        {
            errors.Add($"Provider {provider.ProviderName} estimation failed for {step.Process}: {ex.Message}");
        }
    }
}
