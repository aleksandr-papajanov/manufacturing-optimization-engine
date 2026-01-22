using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Models.Data;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Gateway.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Data.Repositories;

/// <summary>
/// Repository for temporary OptimizationStrategy entities in Gateway.
/// Works directly with OptimizationStrategyEntity.
/// </summary>
public class OptimizationStrategyRepository : Repository<OptimizationStrategyEntity>, IOptimizationStrategyRepository
{
    public OptimizationStrategyRepository(GatewayDbContext context) : base(context)
    {
    }

    public void StoreStrategies(Guid requestId, List<OptimizationStrategyEntity> strategies)
    {
        _dbSet.AddRange(strategies);
        _context.SaveChanges();
    }

    public List<OptimizationStrategyEntity> GetStrategies(Guid requestId)
    {
        return _dbSet
            .Include(s => s.Steps)
                .ThenInclude(st => st.Estimate)
            .Include(s => s.Metrics)
            .Include(s => s.Warranty)
            .Where(s => s.RequestId == requestId && s.PlanId == null) // Strategies not yet assigned to a plan
            .ToList();
    }

    public void RemoveStrategies(Guid requestId)
    {
        var entities = _dbSet.Where(s => s.Plan == null).ToList();
        _dbSet.RemoveRange(entities);
        _context.SaveChanges();
    }
}
