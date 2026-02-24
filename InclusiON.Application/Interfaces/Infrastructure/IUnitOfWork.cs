namespace InclusiON.Application.Interfaces.Infrastructure
{
    public interface IUnitOfWork : IDisposable, IAsyncDisposable
    {
        Task ExecuteInTransactionAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default);
    }
}
