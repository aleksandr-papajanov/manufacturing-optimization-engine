using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;
using ManufacturingOptimization.Common.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Confirms accepted proposals with providers after strategy selection.
/// Sends final confirmation to providers that their proposals have been selected.
/// Works with ProcessStepEntity.
/// </summary>
public class ConfirmationStep : IWorkflowStep
{
    private readonly IMessagePublisher _messagePublisher;

    public ConfirmationStep(IMessagePublisher messagePublisher)
    {
        _messagePublisher = messagePublisher;
    }

    public string Name => "Process Confirmation";

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (context.SelectedStrategy == null)
        {
            throw new InvalidOperationException("No strategy selected for confirmation");
        }
        
        if (!context.PlanId.HasValue)
        {
            throw new InvalidOperationException("Plan ID is required for confirmation step");
        }

        var confirmationTasks = context.SelectedStrategy.Steps.Select(async step =>
        {
            var confirmation = new ConfirmProcessProposalCommand
            {
                ProposalId = step.Estimate.ProposalId
            };

            var response = await _messagePublisher.RequestReplyAsync<ProcessProposalReviewedEvent>(
                Exchanges.Process,
                $"process.confirm.{step.SelectedProviderId}",
                confirmation,
                TimeSpan.FromSeconds(10));

            if (response == null)
                throw new InvalidOperationException($"Provider {step.SelectedProviderId} failed to confirm {step.Process}: No response received");

            if (!response.IsAccepted)
                throw new InvalidOperationException($"Provider {step.SelectedProviderId} declined confirmation for {step.Process}: {response.DeclineReason}");
        });

        await Task.WhenAll(confirmationTasks);
    }
}
