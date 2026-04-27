using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IAdminInstitutionRepository
    {
        Task<List<int>> GetActiveInstitutionIdsByAdminAsync(Guid adminUserId, CancellationToken cancellationToken = default);
        Task<List<User>> GetAllAdminsWithInstitutionsAsync(CancellationToken cancellationToken = default);
        Task<List<AdminInstitution>> GetInstitutionsByAdminAsync(Guid adminUserId, CancellationToken cancellationToken = default);
        Task<AdminInstitution?> FindAssignmentAsync(Guid adminUserId, int institutionId, CancellationToken cancellationToken = default);
        Task AddAsync(AdminInstitution entity, CancellationToken cancellationToken = default);
        void Remove(AdminInstitution entity);
    }
}
