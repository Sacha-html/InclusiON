using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories
{
    public interface IActivityAssignmentRepository
    {
        Task<ActivityAssignment?> GetByIdAsync(int id, CancellationToken ct = default);
        Task<List<ActivityAssignment>> GetByPersonIdAsync(Guid personId, CancellationToken ct = default);
        Task<ActivityAssignment> CreateAsync(ActivityAssignment assignment, CancellationToken ct = default);
        Task UpdateAsync(ActivityAssignment assignment, CancellationToken ct = default);

        Task<ActivityResponse?> GetResponseByIdAsync(int responseId, CancellationToken ct = default);
        Task<ActivityResponse> CreateResponseAsync(ActivityResponse response, CancellationToken ct = default);
        Task UpdateResponseAsync(ActivityResponse response, CancellationToken ct = default);
        Task<int> CountResponsesAsync(int assignmentId, CancellationToken ct = default);

        /// <summary>
        /// Últimas N respuestas completadas de una persona, con actividad incluida.
        /// </summary>
        Task<List<ActivityResponse>> GetRecentCompletedResponsesAsync(
            Guid personId, int limit, CancellationToken ct = default);

        /// <summary>
        /// Últimas N respuestas completadas para un conjunto de personas en una sola query.
        /// Devuelve un diccionario PersonId → respuestas (máximo <paramref name="limit"/> por persona).
        /// </summary>
        Task<Dictionary<Guid, List<ActivityResponse>>> GetRecentCompletedResponsesByPersonIdsAsync(
            IEnumerable<Guid> personIds, int limit, CancellationToken ct = default);
        Task<bool> HasActiveAssignmentAsync(Guid personId, int activityId, CancellationToken ct = default);

        /// <summary>
        /// Persiste una nueva sesión analítica de actividad finalizada.
        /// </summary>
        Task CreateSessionAsync(ActivitySession session, CancellationToken ct = default);

        /// <summary>
        /// Obtiene el progreso del Roadmap, nivel actual y estado GAS para un conjunto de alumnos.
        /// </summary>
        Task<Dictionary<Guid, PersonRoadmapProgress>> GetRoadmapProgressByPersonIdsAsync(
            IEnumerable<Guid> personIds, CancellationToken ct = default);
    }

    /// <summary>
    /// Paso o nivel individual del Roadmap para la visualización del tutor (Mi Camino).
    /// </summary>
    public record RoadmapLevelStep(
        int Level,
        string Title,
        string Status); // "completed" | "struggling" | "active" | "locked"

    /// <summary>
    /// Resumen de avance del Roadmap y estado GAS para el panel familiar.
    /// </summary>
    public record PersonRoadmapProgress(
        int CurrentLevel,
        string LevelName,
        string GasStatusLabel,
        decimal AverageSuccessRate,
        bool HasFrustrationAlert,
        IReadOnlyList<RoadmapLevelStep>? Levels = null);
}
