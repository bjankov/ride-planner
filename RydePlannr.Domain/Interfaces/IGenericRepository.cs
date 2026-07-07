namespace RydePlannr.Domain.Interfaces;
public interface IGenericRepository<TEntity> where TEntity : class
{
    Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(TEntity entity);
    Task DeleteAsync(TEntity entity);
    Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default);
}