using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IFamilyRepository
    {
        Task<FamilyRepresentative?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<FamilyRepresentative?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<bool> ExistsDocumentAsync(string documentNumber, Guid? excludeId = null, CancellationToken cancellationToken = default);
        Task<FamilyRepresentative> CreateAsync(FamilyRepresentative representative, CancellationToken cancellationToken = default);
        Task UpdateAsync(FamilyRepresentative representative, CancellationToken cancellationToken = default);
        Task<PagedResponse<FamilyRepresentative>> GetPagedAsync(
            int page, int pageSize, string? search, bool? isActive,
            SortField? sortBy, string sortDirection,
            List<int>? institutionIds = null,
            CancellationToken cancellationToken = default);
    }
}
