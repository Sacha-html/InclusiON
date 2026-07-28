namespace InclusiON.Application.Interfaces.Common
{
    public interface IQueryHandler<TQuery, TResult>
    {
        Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken);
    }
}
