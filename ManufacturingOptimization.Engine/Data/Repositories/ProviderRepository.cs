using ManufacturingOptimization.Common.Models;
using ManufacturingOptimization.Common.Models.Data;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Engine.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Engine.Data.Repositories;

/// <summary>
/// Repository for Provider entities in Engine.
/// Works directly with ProviderEntity.
/// </summary>
public class ProviderRepository : Repository<ProviderEntity>, IProviderRepository
{
    public ProviderRepository(EngineDbContext context) : base(context)
    {
    }

    public async Task<List<(ProviderEntity ProviderEntity, ProcessCapabilityEntity Capability)>> FindByProcess(ProcessType process)
    {
        var entities = await _dbSet
            .Include(p => p.ProcessCapabilities)
            .Include(p => p.TechnicalCapabilities)
            .Where(p => p.ProcessCapabilities.Any(cap => cap.Process == process.ToString()))
            .ToListAsync();

        var result = new List<(ProviderEntity ProviderEntity, ProcessCapabilityEntity Capability)>();
        foreach (var entity in entities)
        {
            foreach (var cap in entity.ProcessCapabilities.Where(c => c.Process == process.ToString()))
            {
                result.Add((entity, cap));
            }
        }
        return result;
    }

    public override async Task<IEnumerable<ProviderEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProcessCapabilities)
            .Include(p => p.TechnicalCapabilities)
            .ToListAsync(cancellationToken);
    }

    public override async Task<ProviderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProcessCapabilities)
            .Include(p => p.TechnicalCapabilities)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
