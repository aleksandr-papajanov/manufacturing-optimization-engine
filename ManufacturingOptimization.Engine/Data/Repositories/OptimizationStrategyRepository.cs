using ManufacturingOptimization.Common.Models.Data;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Engine.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Engine.Data.Repositories;

/// <summary>
/// Repository for OptimizationStrategy entities in Engine.
/// </summary>
public class OptimizationStrategyRepository : Repository<OptimizationStrategyEntity>, IOptimizationStrategyRepository
{
    public OptimizationStrategyRepository(EngineDbContext context) : base(context)
    {
    }

    public async Task<OptimizationStrategyEntity?> GetByStrategyIdAsync(Guid strategyId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Steps)
                .ThenInclude(st => st.Estimate)
            .Include(s => s.Metrics)
            .Include(s => s.Warranty)
            .FirstOrDefaultAsync(s => s.Id == strategyId, cancellationToken);
    }

    public async Task<IEnumerable<OptimizationStrategyEntity>> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Steps)
                .ThenInclude(st => st.Estimate)
            .Include(s => s.Metrics)
            .Include(s => s.Warranty)
            .Where(s => s.Plan != null && s.Plan.RequestId == requestId)
            .ToListAsync(cancellationToken);
    }

    public override async Task<OptimizationStrategyEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Steps)
                .ThenInclude(st => st.Estimate)
            .Include(s => s.Metrics)
            .Include(s => s.Warranty)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<OptimizationStrategyEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(s => s.Steps)
                .ThenInclude(st => st.Estimate)
            .Include(s => s.Metrics)
            .Include(s => s.Warranty)
            .ToListAsync(cancellationToken);
    }
}
