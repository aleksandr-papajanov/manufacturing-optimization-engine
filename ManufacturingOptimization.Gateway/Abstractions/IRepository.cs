namespace ManufacturingOptimization.Gateway.Abstractions;

/// <summary>
/// Base repository interface for common CRUD operations.
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
/// <typeparam name="TKey">Primary key type</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : class
{
    void Create(TEntity entity);
    TEntity? GetById(TKey id);
    List<TEntity> GetAll();
    void Update(TEntity entity);
    void Delete(TKey id);
}
