namespace InclusiON.Application.Interfaces.Common
{
    public interface ICommandHandler<TCommand, TResult>
    {
        Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken);
    }
}
