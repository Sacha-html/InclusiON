using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Application.Interfaces.Repositories
{
    /// <summary>
    /// Repositorio para operaciones CRUD de profesionales.
    /// </summary>
    public interface IProfessionalsRepository
    {
        /// <summary>
        /// Obtiene un profesional por su ID con las relaciones necesarias.
        /// </summary>
        Task<Professional?> GetByIdAsync(Guid professionalId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene un profesional por su UserId.
        /// </summary>
        Task<Professional?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe un documento duplicado.
        /// </summary>
        Task<bool> ExistsDocumentAsync(string documentNumber, Guid? excludeProfessionalId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe un numero de matricula duplicado.
        /// </summary>
        Task<bool> ExistsLicenseNumberAsync(string licenseNumber, Guid? excludeProfessionalId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Verifica si existe un email registrado en profesionales pendientes.
        /// </summary>
        Task<bool> ExistsProfessionalEmailAsync(string email, Guid? excludeProfessionalId = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Crea un nuevo profesional en la base de datos.
        /// </summary>
        Task<Professional> CreateAsync(Professional professional, CancellationToken cancellationToken = default);

        /// <summary>
        /// Actualiza un profesional existente.
        /// </summary>
        Task UpdateAsync(Professional professional, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene lista paginada de profesionales con filtros.
        /// </summary>
        Task<PagedResponse<Professional>> GetPagedAsync(
            int page,
            int pageSize,
            string? search,
            string? specialty,
            bool? isActive,
            string? status,
            SortField? sortBy,
            string sortDirection,
            List<int>? institutionIds = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene las instituciones asociadas a un profesional.
        /// </summary>
        Task<List<int>> GetInstitutionIdsAsync(Guid professionalId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene profesionales pendientes con paginación.
        /// </summary>
        Task<PagedResponse<Professional>> GetPendingPagedAsync(
            int page,
            int pageSize,
            string? search,
            SortField? sortBy,
            string sortDirection,
            List<int>? institutionIds = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el conteo de profesionales pendientes.
        /// </summary>
        Task<int> GetPendingCountAsync(List<int>? institutionIds = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Agrega un registro al historial de estado del profesional.
        /// </summary>
        Task AddStatusHistoryAsync(ProfessionalStatusHistory history, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene el historial de estados de un profesional.
        /// </summary>
        Task<List<ProfessionalStatusHistory>> GetStatusHistoryAsync(Guid professionalId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene profesionales aprobados que no han iniciado sesión en los últimos días.
        /// </summary>
        Task<List<Professional>> GetInactiveProfessionalsAsync(int inactiveDays, CancellationToken cancellationToken = default);

        /// <summary>
        /// Obtiene todos los profesionales activos y aprobados (para jobs de batch como reportes semanales).
        /// Incluye User para acceder al email.
        /// </summary>
        Task<List<Professional>> GetAllActiveAsync(CancellationToken cancellationToken = default);
        Task<int> GetDependentAssistedLoginPersonsCountAsync(Guid professionalUserId, CancellationToken ct = default);
        Task DeactivateAssignmentsAndCancelActivitiesAsync(Guid professionalUserId, CancellationToken ct = default);
    }
}
