using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Messaging.Messages.ProcessManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ManufacturingOptimization.Engine.Services.Execution;

/// <summary>
/// Orchestrates Phase 2: Execution.
/// Manages the lifecycle of an active plan by coordinating providers step-by-step.
/// </summary>
public class PlanCoordinator : IMessageHandler<OptimizationPlanReadyEvent>, 
                               IMessageHandler<ProcessExecutionCompletedEvent>
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessagePublisher _publisher;
    private readonly ILogger<PlanCoordinator> _logger;

    public PlanCoordinator(IServiceScopeFactory scopeFactory, IMessagePublisher publisher, ILogger<PlanCoordinator> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _logger = logger;
    }

    // ----------------------------------------------------------------------------------
    // TRIGGER: A Plan is Ready (Phase 1 Complete) -> Start Phase 2 (Execution)
    // ----------------------------------------------------------------------------------
    public async Task HandleAsync(OptimizationPlanReadyEvent message)
    {
        var planId = message.Plan.PlanId;
     
        _logger.LogInformation("🚀 [Execution Start] Received Ready Plan {PlanId}. Initializing execution...", planId);

        using var scope = _scopeFactory.CreateScope();
        var planRepo = scope.ServiceProvider.GetRequiredService<IOptimizationPlanRepository>();

        // 1. Reload plan from DB to ensure latest state
        var plan = await planRepo.GetByIdAsync(planId);
        if (plan == null)
        {
            _logger.LogError("❌ Plan {PlanId} not found in database.", planId);
            return;
        }

        // 2. Set Status to InProgress
        plan.Status = OptimizationPlanStatus.InProgress.ToString(); 
        await planRepo.UpdateAsync(plan);

        // 3. Start Step 0
        if (plan.Strategy != null && plan.Strategy.Steps.Any())
        {
            await ExecuteStepAsync(plan, 0);
        }
        else
        {
            _logger.LogWarning("⚠️ Plan {PlanId} has no steps. Marking as Completed.", plan.Id);
            plan.Status = OptimizationPlanStatus.Completed.ToString();
            await planRepo.UpdateAsync(plan);
        }
    }

    // ----------------------------------------------------------------------------------
    // PROGRESSION: A Provider Finished a Step -> Move to Next Step
    // ----------------------------------------------------------------------------------
    public async Task HandleAsync(ProcessExecutionCompletedEvent message)
    {
        _logger.LogInformation("✅ [Step Complete] Received completion for Step {StepId} (Plan {PlanId})", message.StepId, message.PlanId);

        using var scope = _scopeFactory.CreateScope();
        var planRepo = scope.ServiceProvider.GetRequiredService<IOptimizationPlanRepository>();

        var plan = await planRepo.GetByIdAsync(message.PlanId);
        if (plan == null) return;

        // 1. Check for failure
        if (!message.Success)
        {
             _logger.LogError("🛑 [Execution Failed] Provider failed step {StepId}. Reason: {Reason}", message.StepId, message.FailureReason);
             plan.Status = OptimizationPlanStatus.Failed.ToString();
             await planRepo.UpdateAsync(plan);
             return;
        }

        // 2. Determine Current Index based on the step ID that just finished
        var completedStepIndex = plan.Strategy.Steps.FindIndex(s => s.Id == message.StepId);
        if (completedStepIndex == -1)
        {
            _logger.LogWarning("⚠️ Unknown step {StepId} reported for Plan {PlanId}", message.StepId, plan.Id);
            return;
        }

        // 3. Determine Next Step
        var nextStepIndex = completedStepIndex + 1;

        if (nextStepIndex < plan.Strategy.Steps.Count)
        {
            // Continue Execution
            await ExecuteStepAsync(plan, nextStepIndex);
        }
        else
        {
            // Finish Execution
            _logger.LogInformation("🏁 [Execution Finished] All steps completed for Plan {PlanId}", plan.Id);
            plan.Status = OptimizationPlanStatus.Completed.ToString();
            await planRepo.UpdateAsync(plan);
        }
    }

    // ----------------------------------------------------------------------------------
    // HELPER: Send Command to Provider
    // ----------------------------------------------------------------------------------
    private async Task ExecuteStepAsync(OptimizationPlanEntity plan, int stepIndex)
    {
        var step = plan.Strategy.Steps[stepIndex];
        var providerId = step.SelectedProviderId;

        _logger.LogInformation("👉 [Next Step] Commanding Provider {ProviderId} to execute '{Process}' (Step {Index}/{Total})", 
            providerId, step.Process, stepIndex + 1, plan.Strategy.Steps.Count);

        var command = new ExecuteProcessCommand
        {
            PlanId = plan.Id,
            StepId = step.Id,
            StepIndex = stepIndex,
            ProcessName = step.Process.ToString(),
            TargetProviderId = providerId,
            DurationHours = step.Estimate.Duration.TotalHours
        };

        // Routing: Target the specific provider using the pattern defined in Step 1
        var routingKey = $"{ProcessRoutingKeys.Execute}.{providerId}";
        
        await _publisher.PublishAsync(Exchanges.Process, routingKey, command);
    }
}