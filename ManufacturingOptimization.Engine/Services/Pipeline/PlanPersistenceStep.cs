using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Models.Contracts;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Common.Models.Enums;
using ManufacturingOptimization.Engine.Abstractions;
using ManufacturingOptimization.Engine.Models;

namespace ManufacturingOptimization.Engine.Services.Pipeline;

/// <summary>
/// Saves the selected optimization plan to the repository and publishes completion event.
/// This step runs after the customer has selected their preferred strategy.
/// </summary>
public class PlanPersistenceStep : IWorkflowStep
{
    private readonly IMapper _mapper;
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly IOptimizationStrategyRepository _strategyRepository;
    private readonly IMessagePublisher _messagePublisher;

    public string Name => "Plan Persistence";

    public PlanPersistenceStep(
        IMapper mapper,
        IOptimizationPlanRepository planRepository,
        IOptimizationStrategyRepository strategyRepository,
        IMessagePublisher messagePublisher)
    {
        _mapper = mapper;
        _planRepository = planRepository;
        _strategyRepository = strategyRepository;
        _messagePublisher = messagePublisher;
    }

    public async Task ExecuteAsync(WorkflowContext context, CancellationToken cancellationToken = default)
    {
        if (context.SelectedStrategy == null)
        {
            throw new InvalidOperationException("Cannot persist plan: no strategy selected");
        }

        if (!context.PlanId.HasValue)
        {
            throw new InvalidOperationException("Cannot persist plan: Plan ID is missing in context");
        }

        // First, create and save the plan (without strategy reference yet)
        var planEntity = new OptimizationPlanEntity
        {
            Id = context.PlanId.Value,
            RequestId = context.Request.RequestId,
            Status = OptimizationPlanStatus.Confirmed.ToString(),
            CreatedAt = DateTime.UtcNow,
            SelectedAt = DateTime.UtcNow
        };

        await _planRepository.AddAsync(planEntity);
        await _planRepository.SaveChangesAsync(cancellationToken);

        // Now that Plan exists in DB, set PlanId on strategy and save it
        context.SelectedStrategy.PlanId = context.PlanId.Value;
        
        // Map strategy model to entity for saving
        var strategyEntity = _mapper.Map<OptimizationStrategyEntity>(context.SelectedStrategy);
        strategyEntity.PlanId = context.PlanId.Value;
        
        await _strategyRepository.AddAsync(strategyEntity);
        await _strategyRepository.SaveChangesAsync(cancellationToken);
        
        // Load plan with full strategy graph for context and event
        var savedPlan = await _planRepository.GetByIdAsync(planEntity.Id, cancellationToken);
        
        // Store in context for subsequent steps
        context.SavedPlan = savedPlan;

        // Map entity to model for event publishing
        var planModel = _mapper.Map<OptimizationPlanModel>(savedPlan);
        
        // Publish plan ready event
        var planReadyEvent = new OptimizationPlanReadyEvent
        {
            CorrelationId = context.Request.RequestId,
            Plan = planModel
        };

        _messagePublisher.Publish(Exchanges.Optimization, OptimizationRoutingKeys.PlanReady, planReadyEvent);

        await Task.CompletedTask;
    }
}
