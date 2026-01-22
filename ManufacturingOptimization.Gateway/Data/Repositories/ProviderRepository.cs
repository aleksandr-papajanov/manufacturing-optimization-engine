using ManufacturingOptimization.Common.Models.Data;
using ManufacturingOptimization.Common.Models.Data.Entities;
using ManufacturingOptimization.Gateway.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Gateway.Data.Repositories;

/// <summary>
/// Repository for Provider entities in Gateway.
/// Works directly with ProviderEntity.
/// </summary>
public class ProviderRepository : Repository<ProviderEntity>, IProviderRepository
{
    public ProviderRepository(GatewayDbContext context) : base(context)
    {
    }

    public override async Task<ProviderEntity?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProcessCapabilities)
            .Include(p => p.TechnicalCapabilities)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public override async Task<IEnumerable<ProviderEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.ProcessCapabilities)
            .Include(p => p.TechnicalCapabilities)
            .ToListAsync(cancellationToken);
    }
}
