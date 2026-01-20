using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Gateway.Abstractions;
using System.Collections.Concurrent;

namespace ManufacturingOptimization.Gateway.Services;

public class InMemoryOptimizationPlanRepository : IOptimizationPlanRepository
{
    private readonly ConcurrentDictionary<Guid, OptimizationPlan> _plans = new();
    private readonly ILogger<InMemoryOptimizationPlanRepository> _logger;

    public InMemoryOptimizationPlanRepository(ILogger<InMemoryOptimizationPlanRepository> logger)
    {
        _logger = logger;
    }

    public void Create(OptimizationPlan plan)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));

        _plans[plan.PlanId] = plan;
        _logger.LogInformation("Stored optimization plan {PlanId} for request {RequestId}", 
            plan.PlanId, plan.RequestId);
    }

    public OptimizationPlan? GetById(Guid planId)
    {
        return _plans.TryGetValue(planId, out var plan) ? plan : null;
    }

    public OptimizationPlan? GetByRequestId(Guid requestId)
    {
        return _plans.Values.FirstOrDefault(p => p.RequestId == requestId);
    }

    public List<OptimizationPlan> GetAll()
    {
        return _plans.Values.OrderByDescending(p => p.CreatedAt).ToList();
    }

    public void Update(OptimizationPlan plan)
    {
        if (plan == null)
            throw new ArgumentNullException(nameof(plan));

        _plans[plan.PlanId] = plan;
        _logger.LogInformation("Updated optimization plan {PlanId}", plan.PlanId);
    }

    public void Delete(Guid planId)
    {
        _plans.TryRemove(planId, out _);
        _logger.LogInformation("Deleted optimization plan {PlanId}", planId);
    }
}
