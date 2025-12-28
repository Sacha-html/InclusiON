using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace InclusiON.ApplicationBusiness.Interfaces.Infrastructure
{
    public class TransactionScope : IAsyncDisposable
    {
        private readonly IUnitOfWork _uow;
        private readonly ILogger<TransactionScope> _logger;
        private bool _committed = false;
        private bool _disposed;

        public TransactionScope(IUnitOfWork uow, ILogger<TransactionScope>? logger = null)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _logger = logger ?? NullLogger<TransactionScope>.Instance;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            if (_committed)
            {
                return;
            }

            await _uow.CommitTransactionAsync(cancellationToken);
            _committed = true;
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed)
            {
                return;
            }

            if (!_committed)
            {
                try
                {
                    await _uow.RollbackTransactionAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to rollback transaction in DisposeAsync");
                }
            }
            _disposed = true;
        }
    }

}
