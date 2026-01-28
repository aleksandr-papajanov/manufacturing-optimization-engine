using Microsoft.EntityFrameworkCore;

namespace ManufacturingOptimization.Common.Models.Data.Abstractions;

public interface IDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
}