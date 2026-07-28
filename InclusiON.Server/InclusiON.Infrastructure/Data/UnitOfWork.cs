using Microsoft.EntityFrameworkCore;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Data;

namespace InclusiON.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private bool _disposed;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default)
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
                try
                {
                    await operation(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);
                }
                catch
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            });
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            => await _context.SaveChangesAsync(cancellationToken);

        public void Dispose()
        {
            if (!_disposed)
            {
                _context.Dispose();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await _context.DisposeAsync();
                _disposed = true;
            }
            GC.SuppressFinalize(this);
        }
    }
}
