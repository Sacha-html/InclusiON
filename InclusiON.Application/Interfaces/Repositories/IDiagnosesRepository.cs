using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IDiagnosesRepository
    {
        Task<Diagnosis?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Diagnosis>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);
        Task<Diagnosis> CreateAsync(Diagnosis diagnosis, CancellationToken cancellationToken = default);
        Task UpdateAsync(Diagnosis diagnosis, CancellationToken cancellationToken = default);
    }
}
