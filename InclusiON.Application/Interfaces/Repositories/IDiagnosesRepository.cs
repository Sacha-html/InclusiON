using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IDiagnosesRepository
    {
        Task<Diagnosis?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<Diagnosis?> GetByIdIgnoreActiveAsync(int id, CancellationToken cancellationToken = default);
        Task<List<Diagnosis>> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);
        Task<PagedResponse<Diagnosis>> GetPagedByPersonIdAsync(Guid personId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Diagnosis> CreateAsync(Diagnosis diagnosis, CancellationToken cancellationToken = default);
        Task UpdateAsync(Diagnosis diagnosis, CancellationToken cancellationToken = default);
    }
}
