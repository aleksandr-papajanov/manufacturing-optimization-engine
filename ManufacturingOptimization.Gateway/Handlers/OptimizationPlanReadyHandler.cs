using AutoMapper;
using ManufacturingOptimization.Common.Messaging.Abstractions;
using ManufacturingOptimization.Common.Messaging.Messages.OptimizationManagement;
using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;

namespace ManufacturingOptimization.Gateway.Handlers;

/// <summary>
/// Handles optimization plan ready events by saving the plan and updating strategy associations.
/// </summary>
public class OptimizationPlanReadyHandler : IMessageHandler<OptimizationPlanReadyEvent>
{
    private readonly IOptimizationPlanRepository _planRepository;
    private readonly IOptimizationStrategyRepository _strategyRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<OptimizationPlanReadyHandler> _logger;

    public OptimizationPlanReadyHandler(
        IOptimizationPlanRepository planRepository,
        IOptimizationStrategyRepository strategyRepository,
        IMapper mapper,
        ILogger<OptimizationPlanReadyHandler> logger)
    {
        _planRepository = planRepository;
        _strategyRepository = strategyRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task HandleAsync(OptimizationPlanReadyEvent evt)
    {
        // Map and save the plan
        var planEntity = _mapper.Map<OptimizationPlanEntity>(evt.Plan);
        await _planRepository.AddAsync(planEntity);
        await _planRepository.SaveChangesAsync();
        
        // Get strategies for this request (not yet assigned to a plan)
        var unusedStrategies = await _strategyRepository.GetForRequesttAsync(evt.Plan.RequestId);
        
        if (unusedStrategies != null)
        {
            foreach (var strategy in unusedStrategies)
            {
                // Update the selected strategy with PlanId
                if (strategy.Id == evt.Plan.SelectedStrategy.Id)
                {
                    strategy.PlanId = planEntity.Id;
                }
            }
            
            // Save changes to assign PlanId
            await _strategyRepository.SaveChangesAsync();
        }
        
        // Remove unused strategies for this request (those without PlanId)
        await _strategyRepository.RemoveForRequestAsync(evt.Plan.RequestId);
    }
}
