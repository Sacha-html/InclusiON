using InclusiON.Domain.Models;

namespace InclusiON.Agents;

public interface IJobHandler
{
    int JobTypeId { get; }
    Task HandleAsync(BackgroundJob job, CancellationToken cancellationToken = default);
}
