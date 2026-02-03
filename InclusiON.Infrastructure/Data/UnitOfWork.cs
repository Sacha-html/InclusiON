using Microsoft.Extensions.Logging;
using InclusiON.ApplicationBusiness.Interfaces.Infrastructure;
using System.Data;

namespace InclusiON.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<UnitOfWork> _logger;

        private IDbConnection? _connection;
        private IDbTransaction? _transaction;
        private bool _disposed;

        public UnitOfWork(IConnectionFactory connectionFactory, ILogger<UnitOfWork> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public bool HasActiveTransaction => _transaction != null && _connection != null;

        public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (HasActiveTransaction)
            {
                _logger.LogWarning("Transaction already active");
                return;
            }

            try
            {
                _connection = await _connectionFactory.CreateConnectionAsync();
                _transaction = _connection.BeginTransaction();
                _logger.LogDebug("Transaction started");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to begin transaction");
                await DisposeResourcesAsync();
                throw;
            }
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            // ✅ En ADO.NET el "SaveChanges" puede ser solo un placeholder
            // porque los comandos se ejecutan inmediatamente
            _logger.LogDebug("SaveChanges called (ADO.NET - commands already executed)");
            await Task.CompletedTask;
        }

        public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
        {
            if (!HasActiveTransaction)
            {
                _logger.LogWarning("No active transaction to commit");
                return;
            }

            try
            {
                _transaction!.Commit();
                _logger.LogDebug("Transaction committed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to commit transaction");

                try
                {
                    await RollbackTransactionAsync(cancellationToken);
                }
                catch (Exception rollbackEx)
                {
                    _logger.LogError(rollbackEx, "Failed to rollback commit failure");
                }
                throw;

            }
            finally
            {
                await DisposeResourcesAsync();
            }
        }

        public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
        {
            if (!HasActiveTransaction)
            {
                _logger.LogWarning("No active transaction to rollback");
                return;
            }

            try
            {
                _transaction!.Rollback();
                _logger.LogDebug("Transaction rolled back");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to rollback transaction");
                throw;
            }
            finally
            {
                await DisposeResourcesAsync();
            }
        }

        public async Task<IDbConnection> GetConnectionAsync()
        {
            if (_connection != null)
                return _connection;

            _connection = await _connectionFactory.CreateConnectionAsync();
            return _connection;
        }

        public IDbTransaction? GetCurrentTransaction() => _transaction;

        private async Task DisposeResourcesAsync()
        {
            if (_transaction != null)
            {
                _transaction.Dispose();
                _transaction = null;
            }

            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Dispose sincrónico para compatibilidad con código legacy
                _transaction?.Dispose();
                _transaction = null;
                _connection?.Dispose();
                _connection = null;
                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                await DisposeResourcesAsync();
                _disposed = true;
            }
        }

        public async Task<TransactionScope> BeginTransactionScopeAsync(CancellationToken cancellationToken = default)
        {
            await BeginTransactionAsync(cancellationToken);
            return new TransactionScope(this);
        }
    }
}
