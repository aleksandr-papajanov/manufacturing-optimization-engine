using ManufacturingOptimization.Common.Models.Data;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.ProviderRegistry.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.ProviderRegistry.Data.Repositories;

/// <summary>
/// Repository for Provider entities in ProviderRegistry.
/// Works directly with ProviderEntity.
/// </summary>
public class ProviderRepository : Repository<ProviderEntity>, IProviderRepository
{
    public ProviderRepository(ProviderRegistryDbContext context) : base(context)
    {
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
