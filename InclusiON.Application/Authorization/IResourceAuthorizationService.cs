namespace InclusiON.Application.Authorization
{
    /// <summary>
    /// Servicio de autorizacion por recurso (row-level authorization).
    /// Evalua si el usuario autenticado tiene vinculo explicito con el recurso solicitado,
    /// segun las reglas definidas en HU-IN-172.
    ///
    /// Reglas por rol:
    /// - <b>GlobalAdmin</b>: bypass (siempre permitido, pero auditado).
    /// - <b>Admin institucional</b>: permitido si la institucion del recurso esta en <c>AdminInstitutions</c>.
    /// - <b>Professional</b>: permitido si existe <c>ProfessionalPerson</c> activa entre el profesional y la persona del recurso.
    /// - <b>FamilyRepresentative</b>: permitido si existe <c>PersonRepresentative</c> activo con la persona del recurso.
    ///   Para login asistido, requiere ademas <c>CanSuperviseLogin = true</c>.
    /// - <b>PersonWithDisability</b>: permitido unicamente sobre sus propios recursos.
    ///
    /// Cada invocacion audita el resultado (Allowed/Denied) via <c>IAccessAuditLogger</c>.
    /// </summary>
    public interface IResourceAuthorizationService
    {
        // === Recurso canonico: Person ===

        /// <summary>
        /// Verifica si el usuario autenticado puede acceder a la persona indicada.
        /// Es el check canonico del que derivan la mayoria de los demas (Report, Diagnosis, etc.).
        /// </summary>
        Task<bool> CanAccessPersonAsync(Guid personId, AccessMode mode, CancellationToken ct = default);

        /// <summary>
        /// Devuelve la lista de <c>PersonId</c> a los que el usuario autenticado tiene acceso en modo lectura.
        /// Usado para filtrar listados en repositorios (evita post-filtrado en memoria).
        /// Para GlobalAdmin retorna la lista completa de personas activas.
        /// </summary>
        Task<IReadOnlyList<Guid>> GetAccessiblePersonIdsAsync(CancellationToken ct = default);

        // === Recursos derivados (resuelven a PersonId y delegan al check canonico) ===

        /// <summary>
        /// Verifica acceso a un reporte de progreso.
        /// Familias solo ven reportes en estado <c>Approved</c> de personas a cargo.
        /// </summary>
        Task<bool> CanAccessReportAsync(int reportId, AccessMode mode, CancellationToken ct = default);

        /// <summary>
        /// Verifica acceso a un diagnostico funcional.
        /// Profesionales no asignados nunca pueden acceder (ni lectura).
        /// </summary>
        Task<bool> CanAccessDiagnosisAsync(int diagnosisId, AccessMode mode, CancellationToken ct = default);

        /// <summary>
        /// Verifica acceso al perfil de habilidades (<c>PersonSkillProfile</c>) de una persona.
        /// </summary>
        Task<bool> CanAccessSkillProfileAsync(Guid personId, AccessMode mode, CancellationToken ct = default);

        /// <summary>
        /// Verifica acceso a una respuesta de actividad (<c>ActivityResponse</c>).
        /// </summary>
        Task<bool> CanAccessResponseAsync(Guid responseId, AccessMode mode, CancellationToken ct = default);

        /// <summary>
        /// Verifica acceso a una asignacion de actividad (<c>ActivityAssignment</c>).
        /// </summary>
        Task<bool> CanAccessActivityAssignmentAsync(Guid assignmentId, AccessMode mode, CancellationToken ct = default);

        /// <summary>
        /// Verifica acceso al roadmap de una persona (<c>PersonRoadmap</c>).
        /// </summary>
        Task<bool> CanAccessRoadmapAsync(Guid personId, AccessMode mode, CancellationToken ct = default);

        // === Recursos especiales ===

        /// <summary>
        /// Verifica acceso a una invitacion.
        /// Profesional: invitaciones creadas por el mismo o dirigidas a personas asignadas.
        /// Admin institucional: invitaciones de su institucion.
        /// Familiar: invitaciones dirigidas a su email.
        /// </summary>
        Task<bool> CanAccessInvitationAsync(int invitationId, AccessMode mode, CancellationToken ct = default);

        /// <summary>
        /// Verifica acceso a los datos de otro usuario (no al propio perfil).
        /// - Professional: usuarios familiares de sus personas asignadas + otros profesionales que comparten persona.
        /// - Family: el usuario de la persona a cargo + otros representantes de la misma persona.
        /// - Admin institucional: usuarios de su institucion.
        /// - El propio usuario siempre tiene acceso a sus datos (atajo).
        /// </summary>
        Task<bool> CanAccessUserAsync(Guid targetUserId, AccessMode mode, CancellationToken ct = default);

        // === Reglas especificas de negocio ===

        /// <summary>
        /// Verifica si el usuario autenticado puede realizar <b>login asistido</b> sobre la persona indicada.
        /// Requiere <c>PersonRepresentative.IsActive = true</c> Y <c>CanSuperviseLogin = true</c>
        /// (o bien ser un profesional asignado con el permiso correspondiente).
        /// </summary>
        Task<bool> CanSuperviseLoginAsync(Guid personId, CancellationToken ct = default);
    }
}
