namespace InclusiON.Agents.Cleanup;

public interface ICleanupStep
{
    Task ExecuteAsync(CancellationToken cancellationToken = default);
}
