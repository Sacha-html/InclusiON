using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IInvitationsRepository
    {
        Task<Invitation?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
        Task<List<Invitation>> GetAllAsync(CancellationToken cancellationToken = default);
        Task<List<Invitation>> GetByProfessionalIdAsync(Guid professionalId, CancellationToken cancellationToken = default);
        Task<List<Invitation>> GetByInstitutionIdsAsync(List<int> institutionIds, CancellationToken cancellationToken = default);
        Task<PagedResponse<Invitation>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<PagedResponse<Invitation>> GetPagedByProfessionalIdAsync(Guid professionalId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<PagedResponse<Invitation>> GetPagedByInstitutionIdsAsync(List<int> institutionIds, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<Invitation> CreateAsync(Invitation invitation, CancellationToken cancellationToken = default);
        Task UpdateAsync(Invitation invitation, CancellationToken cancellationToken = default);
        Task<FamilyRepresentative> CreateFamilyRepresentativeAsync(FamilyRepresentative representative, CancellationToken cancellationToken = default);
        Task CreatePersonRepresentativeAsync(PersonRepresentative personRepresentative, CancellationToken cancellationToken = default);
    }
}
