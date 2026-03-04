using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Application.Interfaces.Repositories.Base
{
    public interface IReadOnlyRepository<TEntity> where TEntity : class, IActivatable
    {
        Task<List<TEntity>> GetAllActiveAsync(CancellationToken cancellationToken = default);
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
