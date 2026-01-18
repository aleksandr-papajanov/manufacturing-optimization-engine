using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Engine.Abstractions;
using System.Collections.Concurrent;

namespace ManufacturingOptimization.Engine.Services;

/// <summary>
/// In-memory implementation of optimization plan repository.
/// Thread-safe storage using ConcurrentDictionary.
/// </summary>
public class InMemoryOptimizationPlanRepository : IOptimizationPlanRepository
{
    private readonly ConcurrentDictionary<Guid, OptimizationPlan> _plansByPlanId = new();
    private readonly ConcurrentDictionary<Guid, Guid> _planIdByRequestId = new();
    private readonly ILogger<InMemoryOptimizationPlanRepository> _logger;

    public InMemoryOptimizationPlanRepository(ILogger<InMemoryOptimizationPlanRepository> logger)
    {
        _logger = logger;
    }

    public OptimizationPlan Create(OptimizationPlan plan)
    {
        if (plan.PlanId == Guid.Empty)
        {
            plan.PlanId = Guid.NewGuid();
        }

        if (_plansByPlanId.TryAdd(plan.PlanId, plan))
        {
            // Also index by request ID for quick lookup
            _planIdByRequestId.TryAdd(plan.RequestId, plan.PlanId);
            
            return plan;
        }

        throw new InvalidOperationException($"Plan with ID {plan.PlanId} already exists");
    }

    public OptimizationPlan? GetById(Guid planId)
    {
        _plansByPlanId.TryGetValue(planId, out var plan);
        return plan;
    }

    public OptimizationPlan? GetByRequestId(Guid requestId)
    {
        if (_planIdByRequestId.TryGetValue(requestId, out var planId))
        {
            return GetById(planId);
        }
        return null;
    }

    public bool Update(OptimizationPlan plan)
    {
        if (!_plansByPlanId.ContainsKey(plan.PlanId))
        {
            return false;
        }

        _plansByPlanId[plan.PlanId] = plan;
        
        return true;
    }

    public IEnumerable<OptimizationPlan> GetAll()
    {
        return _plansByPlanId.Values;
    }
}
