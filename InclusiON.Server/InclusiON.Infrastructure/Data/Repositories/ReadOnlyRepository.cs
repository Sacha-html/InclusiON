using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Data;
using InclusiON.Domain.Models.BaseEntities;
using System.Linq.Expressions;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class ReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class, IActivatable, IHasIntId
    {
        private readonly AppDbContext _context;

        public ReadOnlyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TEntity>> GetAllActiveAsync(CancellationToken cancellationToken = default,
            params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> query = _context.Set<TEntity>();
            foreach (var include in includes)
                query = query.Include(include);

            return await query
                .Where(x => x.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
    }
}
