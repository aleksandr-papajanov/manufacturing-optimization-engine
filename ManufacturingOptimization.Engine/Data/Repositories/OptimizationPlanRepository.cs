using ManufacturingOptimization.Common.Models.Data;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Engine.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Engine.Data.Repositories;

/// <summary>
/// Repository for OptimizationPlan entities in Engine.
/// Works directly with OptimizationPlanEntity.
/// </summary>
public class OptimizationPlanRepository : Repository<OptimizationPlanEntity>, IOptimizationPlanRepository
{
    public OptimizationPlanRepository(EngineDbContext context) : base(context)
    {
    }

    public async Task<OptimizationPlanEntity?> GetByRequestIdAsync(Guid requestId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.Estimate)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Metrics)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Warranty)
            .FirstOrDefaultAsync(p => p.RequestId == requestId, cancellationToken);
    }

    public override async Task<OptimizationPlanEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.Estimate)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Metrics)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Warranty)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<OptimizationPlanEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Steps)
                    .ThenInclude(st => st.Estimate)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Metrics)
            .Include(p => p.SelectedStrategy)
                .ThenInclude(s => s.Warranty)
            .ToListAsync(cancellationToken);
    }
}
