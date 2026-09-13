using InclusiON.Domain.Models.BaseEntities;
using System.Linq.Expressions;

namespace InclusiON.Application.Interfaces.Repositories.Base
{
    public interface IReadOnlyRepository<TEntity> where TEntity : class, IActivatable, IHasIntId
    {
        Task<List<TEntity>> GetAllActiveAsync(CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes);
        Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    }
}
