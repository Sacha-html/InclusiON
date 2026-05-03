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
            string? linkedPersonSearch = null,
            CancellationToken cancellationToken = default);

        Task<List<(FamilyRepresentative Family, bool WasPreviouslyLinked)>> GetAvailableFamiliesAsync(string? search = null, Guid? personId = null, CancellationToken cancellationToken = default);
        Task<List<PersonRepresentative>> GetPersonRepresentativesByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);
        Task<List<PersonRepresentative>> GetPersonRepresentativesByFamilyIdAsync(Guid familyId, CancellationToken cancellationToken = default);
        Task<PersonRepresentative?> GetPersonRepresentativeAsync(Guid personId, Guid representativeId, CancellationToken cancellationToken = default);
        Task CreatePersonRepresentativeAsync(PersonRepresentative personRepresentative, CancellationToken cancellationToken = default);
        Task UpdatePersonRepresentativeAsync(PersonRepresentative personRepresentative, CancellationToken cancellationToken = default);
        Task DeletePersonRepresentativeAsync(PersonRepresentative personRepresentative, CancellationToken cancellationToken = default);
        Task CreateFamilyStatusHistoryAsync(FamilyStatusHistory history, CancellationToken cancellationToken = default);
        Task<List<FamilyStatusHistory>> GetFamilyStatusHistoryAsync(Guid familyId, CancellationToken cancellationToken = default);
        Task CreatePersonRepresentativeHistoryAsync(PersonRepresentativeHistory history, CancellationToken cancellationToken = default);
        Task<List<PersonRepresentativeHistory>> GetPersonRepresentativeHistoryAsync(Guid personId, CancellationToken cancellationToken = default);
        Task<List<PersonRepresentativeHistory>> GetPersonRepresentativeHistoryByFamilyAsync(Guid familyId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Devuelve las personas con discapacidad vinculadas activamente al familiar (por UserId).
        /// </summary>
        Task<List<PersonWithDisability>> GetLinkedPersonsAsync(Guid familyUserId, CancellationToken cancellationToken = default);
    }
}
