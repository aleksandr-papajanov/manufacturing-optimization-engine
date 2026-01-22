using ManufacturingOptimization.Common.Models.Data.Abstractions;
using ManufacturingOptimization.Common.Models.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Models.Data.Repositories;

/// <summary>
/// Repository for temporary OptimizationStrategy entities in Gateway.
/// Works directly with OptimizationStrategyEntity.
/// </summary>
public class OptimizationStrategyRepository : Repository<OptimizationStrategyEntity>, IOptimizationStrategyRepository
{
    public OptimizationStrategyRepository(IOptimizationDbContext context) : base(context)
    {
    }

    public async Task AddForRequestAsync(Guid requestId, List<OptimizationStrategyEntity> strategies)
    {
        _dbSet.AddRange(strategies);
        await _context.SaveChangesAsync();
    }

    public async Task<List<OptimizationStrategyEntity>?> GetForRequesttAsync(Guid requestId)
    {
        return await _dbSet
            .Include(s => s.Steps)
                .ThenInclude(st => st.Estimate)
            .Include(s => s.Metrics)
            .Include(s => s.Warranty)
            .Where(s => s.RequestId == requestId && s.PlanId == null) // Strategies not yet assigned to a plan
            .ToListAsync();
    }

    public async Task RemoveForRequestAsync(Guid requestId)
    {
        var entities = await _dbSet.Where(s => s.Plan == null).ToListAsync();
        _dbSet.RemoveRange(entities);
        await _context.SaveChangesAsync();
    }
}
