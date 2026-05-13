using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IActivitiesRepository
    {
        Task<Activity?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<(List<Activity> Items, int Total)> GetPagedAsync(
            Guid professionalId,
            string? search,
            int? categoryId,
            int? skillAreaId,
            int? templateTypeId,
            bool? isActive,
            bool? isStandard,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task<Activity> CreateAsync(Activity activity, CancellationToken cancellationToken = default);
        Task UpdateAsync(Activity activity, CancellationToken cancellationToken = default);
        Task<bool> HasActiveAssignmentsAsync(int activityId, CancellationToken cancellationToken = default);
        Task<List<Activity>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
    }
}
