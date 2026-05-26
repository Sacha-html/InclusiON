using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Infrastructure;

public interface IJobHandler
{
    int JobTypeId { get; }
    Task HandleAsync(BackgroundJob job, CancellationToken cancellationToken = default);
}
