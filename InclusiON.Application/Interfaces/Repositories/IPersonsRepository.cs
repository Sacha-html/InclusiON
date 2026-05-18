using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones CRUD de personas con discapacidad.
    /// </summary>
    public interface IPersonsRepository
    {
        /// <summary>
        /// Obtiene una persona por su ID con todas las relaciones necesarias.
        /// </summary>
        Task<PersonWithDisability?> GetByIdAsync(Guid personId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una persona por su UserId.
        /// </summary>
        Task<PersonWithDisability?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe un documento duplicado.
        /// </summary>
        Task<bool> ExistsDocumentAsync(string documentNumber, Guid? excludePersonId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea una nueva persona en la base de datos.
        /// </summary>
        Task<PersonWithDisability> CreateAsync(PersonWithDisability person, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza una persona existente.
        /// </summary>
        Task UpdateAsync(PersonWithDisability person, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los profesionales asignados a una persona que pueden supervisar el login asistido.
        /// Filtra por asignación activa + flag CanSuperviseLogin.
        /// </summary>
        Task<IReadOnlyList<Professional>> GetSupervisingProfessionalsAsync(Guid personId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene los representantes familiares activos vinculados a una persona.
        /// Incluye el FamilyRepresentative y la relación (parentesco).
        /// </summary>
        Task<IReadOnlyList<PersonRepresentative>> GetActiveRepresentativesAsync(Guid personId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el perfil de habilidades de una persona.
        /// </summary>
        Task<List<PersonSkillProfile>> GetSkillProfileAsync(
            Guid personId,
            bool activeOnly,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene una entrada concreta del perfil de habilidades (con tracking para mutaciones).
        /// Incluye la SkillArea para el mapeo de respuesta.
        /// </summary>
        Task<PersonSkillProfile?> GetSkillProfileEntryAsync(
            Guid personId,
            int skillAreaId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega una nueva entrada al perfil de habilidades de una persona.
        /// </summary>
        Task AddSkillProfileEntryAsync(
            PersonSkillProfile entry,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene lista paginada de personas con filtros.
        /// </summary>
        Task<PagedResponse<PersonWithDisability>> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            int? disabilityTypeId,
            int? autonomyLevelId,
            bool? isActive,
            SortField? sortBy,
            string sortDirection,
            List<int>? institutionIds = null,
            string? representativeSearch = null,
            IReadOnlyList<Guid>? accessiblePersonIds = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene múltiples personas por sus IDs (preservando orden).
        /// </summary>
        Task<List<PersonWithDisability>> GetByIdsAsync(
            List<Guid> ids,
            CancellationToken cancellationToken = default);
    }
}
