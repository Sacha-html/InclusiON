using InclusiON.Domain.Models;

namespace InclusiON.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones sobre roadmaps personalizados.
    /// </summary>
    public interface IRoadmapRepository
    {
        /// <summary>
        /// Obtiene el roadmap completo de una persona, incluyendo areas y actividades.
        /// </summary>
        Task<PersonRoadmap?> GetByPersonIdAsync(Guid personId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Indica si una persona ya tiene un roadmap creado.
        /// </summary>
        Task<bool> ExistsForPersonAsync(Guid personId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea un nuevo roadmap.
        /// </summary>
        Task<PersonRoadmap> CreateAsync(PersonRoadmap roadmap, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un area del roadmap por su ID (con tracking para mutaciones).
        /// </summary>
        Task<PersonRoadmapArea?> GetAreaByIdAsync(int areaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Indica si un area de habilidad ya existe en el roadmap indicado.
        /// </summary>
        Task<bool> AreaExistsInRoadmapAsync(int roadmapId, int skillAreaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega un area al roadmap.
        /// </summary>
        Task AddAreaAsync(PersonRoadmapArea area, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina un area del roadmap (con sus actividades en cascada).
        /// </summary>
        void RemoveArea(PersonRoadmapArea area);

        /// <summary>
        /// Obtiene una actividad asignada al roadmap por su ID (con tracking para mutaciones).
        /// </summary>
        Task<PersonRoadmapActivity?> GetActivityByIdAsync(int activityEntryId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Indica si una actividad ya existe en el area del roadmap indicada.
        /// </summary>
        Task<bool> ActivityExistsInAreaAsync(int roadmapAreaId, int activityId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todas las actividades de un area del roadmap (para reordenar).
        /// </summary>
        Task<List<Domain.Models.PersonRoadmapActivity>> GetActivitiesByAreaIdAsync(int areaId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega una actividad a un area del roadmap.
        /// </summary>
        Task AddActivityAsync(PersonRoadmapActivity activity, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina una actividad asignada al roadmap.
        /// </summary>
        void RemoveActivity(PersonRoadmapActivity activity);

        /// <summary>
        /// Busca la entrada de roadmap que vincula a una persona con una actividad específica.
        /// Usado para determinar el umbral de desbloqueo al completar una actividad.
        /// </summary>
        Task<PersonRoadmapActivity?> GetByPersonAndActivityAsync(Guid personId, int activityId, CancellationToken ct = default);

        /// <summary>
        /// Obtiene la siguiente actividad en secuencia dentro del mismo área del roadmap.
        /// </summary>
        Task<PersonRoadmapActivity?> GetNextInAreaAsync(int areaId, int currentSequenceOrder, CancellationToken ct = default);
    }
}
