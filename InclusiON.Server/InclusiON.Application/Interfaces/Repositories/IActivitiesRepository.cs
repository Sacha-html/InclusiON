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
            bool? isTemplate,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task<Activity> CreateAsync(Activity activity, CancellationToken cancellationToken = default);
        Task UpdateAsync(Activity activity, CancellationToken cancellationToken = default);
        Task<bool> HasActiveAssignmentsAsync(int activityId, CancellationToken cancellationToken = default);
        Task<List<Activity>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken = default);
        Task<List<ActivityEmbeddingProjection>> GetAllActiveForEmbeddingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Devuelve las actividades estándar activas (IsStandardActivity = true) para regeneración nocturna de embeddings.
        /// </summary>
        Task<List<ActivityEmbeddingProjection>> GetStandardActivitiesForEmbeddingAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Devuelve la lista ordenada de plantillas del Roadmap oficial (IsTemplate = true y RoadmapOrder != null).
        /// </summary>
        Task<List<Activity>> GetRoadmapTemplatesAsync(CancellationToken cancellationToken = default);
    }

    public record ActivityEmbeddingProjection(
        int Id,
        string Title,
        string? Description,
        string? Instructions,
        string? ContentJson);
}
