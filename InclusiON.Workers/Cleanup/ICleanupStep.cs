namespace InclusiON.Workers.Cleanup;

public interface ICleanupStep
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
