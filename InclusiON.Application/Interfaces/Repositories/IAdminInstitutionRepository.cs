namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IAdminInstitutionRepository
    {
        Task<List<int>> GetActiveInstitutionIdsByAdminAsync(Guid adminUserId, CancellationToken cancellationToken = default);
    }
}
