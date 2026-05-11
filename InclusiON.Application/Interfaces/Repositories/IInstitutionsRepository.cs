using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones CRUD de instituciones educativas.
    /// </summary>
    public interface IInstitutionsRepository
    {
        Task<List<EducationalInstitution>> GetAllAsync(CancellationToken ct = default);
        Task<PagedResponse<EducationalInstitution>> GetPagedAsync(int page, int pageSize, string? search, bool? isActive, CancellationToken ct = default);
        Task<EducationalInstitution?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<EducationalInstitution> CreateAsync(EducationalInstitution institution, CancellationToken ct = default);
        Task UpdateAsync(EducationalInstitution institution, CancellationToken ct = default);
        Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken ct = default);
        Task<bool> HasActiveProfessionalsAsync(int institutionId, CancellationToken ct = default);
    }
}
