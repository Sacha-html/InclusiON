using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Data;
using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Infrastructure.Data.Repositories
{
    public class ReadOnlyRepository<TEntity> : IReadOnlyRepository<TEntity>
        where TEntity : class, IActivatable
    {
        private readonly AppDbContext _context;

        public ReadOnlyRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<TEntity>> GetAllActiveAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Set<TEntity>()
                .Where(x => x.IsActive)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }

        public async Task<TEntity?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Set<TEntity>().FindAsync([id], cancellationToken);
        }
    }
}
