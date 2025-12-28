using InclusiON.DTOs.Common;

namespace InclusiON.ApplicationBusiness.Interfaces.Repositories.Base
{
    public interface IBaseRepository<TEntity, TKey, TFilter>
        where TEntity : class
        where TFilter : class
    {
        Task<TKey> AddAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
        Task DeleteAsync(TKey id, CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
        Task<PagedResponse<TEntity>> GetAllAsync(TFilter filter, CancellationToken cancellationToken = default);
    }
}
