using System.Data;

namespace InclusiON.ApplicationBusiness.Interfaces.Infrastructure
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        Task BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task CommitTransactionAsync(CancellationToken cancellationToken = default);
        Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

        Task<IDbConnection> GetConnectionAsync();
        IDbTransaction? GetCurrentTransaction();
        bool HasActiveTransaction { get; }
        Task<TransactionScope> BeginTransactionScopeAsync(CancellationToken cancellationToken = default);
    }
}
