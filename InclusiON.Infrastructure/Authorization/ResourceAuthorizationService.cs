using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Auditing;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Data;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Infrastructure.Authorization
{
    /// <summary>
    /// Implementacion de <see cref="IResourceAuthorizationService"/>.
    /// Scoped: cachea lookups de ProfessionalId / FamilyRepresentativeId y la lista
    /// de PersonIds accesibles durante el request para evitar N+1.
    ///
    /// Fase 1 (HU-IN-172): implementadas las operaciones canonicas sobre Person.
    /// Las derivadas (Report, Diagnosis, etc.) se implementaran a medida que se
    /// instrumenten los controllers correspondientes.
    /// </summary>
    public class ResourceAuthorizationService : IResourceAuthorizationService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextService _httpContext;
        private readonly IAccessAuditLogger _auditLogger;
        private readonly ILogger<ResourceAuthorizationService> _logger;

        // Cache por request (scoped)
        private Task<IReadOnlyList<Guid>>? _accessibleIdsCache;

        public ResourceAuthorizationService(
            AppDbContext context,
            IHttpContextService httpContext,
            IAccessAuditLogger auditLogger,
            ILogger<ResourceAuthorizationService> logger)
        {
            _context = context;
            _httpContext = httpContext;
            _auditLogger = auditLogger;
            _logger = logger;
        }

        // ==================================================================
        // Canonicos (implementados)
        // ==================================================================

        public async Task<bool> CanAccessPersonAsync(Guid personId, AccessMode mode, CancellationToken ct = default)
        {
            var userId = _httpContext.GetCurrentUserId();
            if (userId is null)
            {
                return false;
            }

            var role = _httpContext.GetCurrentUserRole();
            var isGlobalAdmin = _httpContext.IsGlobalAdmin();

            bool allowed;

            if (isGlobalAdmin)
            {
                allowed = true;
            }
            else
            {
                var accessibleIds = await GetAccessiblePersonIdsAsync(ct);
                allowed = accessibleIds.Contains(personId);
            }

            await _auditLogger.LogAsync(new AccessAuditEntry
            {
                UserId = userId.Value,
                Role = role,
                AccessedPersonId = personId,
                ActionType = mode == AccessMode.Write ? AccessAuditValues.Action.Update : AccessAuditValues.Action.Read,
                Result = allowed ? AccessAuditValues.Result.Allowed : AccessAuditValues.Result.Denied,
                AffectedTable = "Persons",
                AffectedRecordId = personId.ToString()
            }, ct);

            return allowed;
        }

        public Task<IReadOnlyList<Guid>> GetAccessiblePersonIdsAsync(CancellationToken ct = default)
        {
            // Cache por request: primera invocacion computa, siguientes reutilizan
            _accessibleIdsCache ??= ComputeAccessiblePersonIdsAsync(ct);
            return _accessibleIdsCache;
        }

        private async Task<IReadOnlyList<Guid>> ComputeAccessiblePersonIdsAsync(CancellationToken ct)
        {
            var userId = _httpContext.GetCurrentUserId();
            if (userId is null)
            {
                return Array.Empty<Guid>();
            }

            if (_httpContext.IsGlobalAdmin())
            {
                return await _context.PersonsWithDisability
                    .Where(p => p.IsActive)
                    .Select(p => p.Id)
                    .ToListAsync(ct);
            }

            var role = _httpContext.GetCurrentUserRole();

            return role switch
            {
                nameof(IdentityRoles.Professional) => await GetPersonIdsForProfessionalAsync(userId.Value, ct),
                nameof(IdentityRoles.FamilyRepresentative) => await GetPersonIdsForFamilyAsync(userId.Value, ct),
                nameof(IdentityRoles.Admin) => await GetPersonIdsForInstitutionalAdminAsync(ct),
                nameof(IdentityRoles.PersonWithDisability) => await GetPersonIdsForSelfAsync(userId.Value, ct),
                _ => Array.Empty<Guid>()
            };
        }

        private async Task<IReadOnlyList<Guid>> GetPersonIdsForProfessionalAsync(Guid userId, CancellationToken ct)
        {
            // Preferir entityId del JWT (evita el JOIN con la tabla Professionals).
            var professionalId = _httpContext.GetCurrentEntityId();
            if (professionalId.HasValue)
            {
                return await _context.ProfessionalPersons
                    .Where(pp => pp.ProfessionalId == professionalId.Value && pp.IsActive)
                    .Select(pp => pp.PersonId)
                    .Distinct()
                    .ToListAsync(ct);
            }

            // Fallback conservador: join por UserId.
            return await _context.ProfessionalPersons
                .Where(pp => pp.Professional.UserId == userId && pp.IsActive)
                .Select(pp => pp.PersonId)
                .Distinct()
                .ToListAsync(ct);
        }

        private async Task<IReadOnlyList<Guid>> GetPersonIdsForFamilyAsync(Guid userId, CancellationToken ct)
        {
            // Preferir entityId del JWT (evita el JOIN con la tabla FamilyRepresentatives).
            var familyId = _httpContext.GetCurrentEntityId();
            if (familyId.HasValue)
            {
                return await _context.PersonRepresentatives
                    .Where(pr => pr.RepresentativeId == familyId.Value && pr.IsActive)
                    .Select(pr => pr.PersonId)
                    .Distinct()
                    .ToListAsync(ct);
            }

            // Fallback conservador: join por UserId.
            return await _context.PersonRepresentatives
                .Where(pr => pr.Representative.UserId == userId && pr.IsActive)
                .Select(pr => pr.PersonId)
                .Distinct()
                .ToListAsync(ct);
        }

        private async Task<IReadOnlyList<Guid>> GetPersonIdsForInstitutionalAdminAsync(CancellationToken ct)
        {
            var institutionIds = _httpContext.GetInstitutionIds();
            if (institutionIds.Count == 0)
            {
                return Array.Empty<Guid>();
            }

            // Persona "pertenece" a una institucion si algun profesional asignado
            // esta vinculado a esa institucion. No hay link directo Persona -> Institucion hoy.
            return await _context.ProfessionalPersons
                .Where(pp => pp.IsActive
                    && _context.ProfessionalInstitutions.Any(pi =>
                        pi.ProfessionalId == pp.ProfessionalId
                        && institutionIds.Contains(pi.InstitutionId)))
                .Select(pp => pp.PersonId)
                .Distinct()
                .ToListAsync(ct);
        }

        private async Task<IReadOnlyList<Guid>> GetPersonIdsForSelfAsync(Guid userId, CancellationToken ct)
        {
            return await _context.PersonsWithDisability
                .Where(p => p.UserId == userId && p.IsActive)
                .Select(p => p.Id)
                .ToListAsync(ct);
        }

        // ==================================================================
        // Derivados (pendientes — se implementan al instrumentar cada controller)
        // ==================================================================

        public async Task<bool> CanAccessFamilyAsync(Guid familyId, AccessMode mode, CancellationToken ct = default)
        {
            var userId = _httpContext.GetCurrentUserId();
            if (userId is null)
            {
                await _auditLogger.LogAsync(new AccessAuditEntry
                {
                    UserId = Guid.Empty,
                    Role = null,
                    AccessedPersonId = null,
                    ActionType = AccessAuditValues.Action.Read,
                    Result = AccessAuditValues.Result.Denied,
                    AffectedTable = "FamilyRepresentatives",
                    AffectedRecordId = familyId.ToString()
                }, ct);
                return false;
            }

            var role = _httpContext.GetCurrentUserRole();
            var isGlobalAdmin = _httpContext.IsGlobalAdmin();

            if (isGlobalAdmin)
            {
                await _auditLogger.LogAsync(new AccessAuditEntry
                {
                    UserId = userId.Value,
                    Role = role,
                    AccessedPersonId = null,
                    ActionType = mode == AccessMode.Write ? AccessAuditValues.Action.Update : AccessAuditValues.Action.Read,
                    Result = AccessAuditValues.Result.Allowed,
                    AffectedTable = "FamilyRepresentatives",
                    AffectedRecordId = familyId.ToString()
                }, ct);
                return true;
            }

            // Self-access: family member viewing/editing their own record
            var entityId = _httpContext.GetCurrentEntityId();
            if (role == nameof(IdentityRoles.FamilyRepresentative) && entityId.HasValue && entityId.Value == familyId)
            {
                await _auditLogger.LogAsync(new AccessAuditEntry
                {
                    UserId = userId.Value,
                    Role = role,
                    AccessedPersonId = null,
                    ActionType = mode == AccessMode.Write ? AccessAuditValues.Action.Update : AccessAuditValues.Action.Read,
                    Result = AccessAuditValues.Result.Allowed,
                    AffectedTable = "FamilyRepresentatives",
                    AffectedRecordId = familyId.ToString()
                }, ct);
                return true;
            }

            // Get linked persons for this family
            var linkedPersonIds = await _context.PersonRepresentatives
                .Where(pr => pr.RepresentativeId == familyId && pr.IsActive)
                .Select(pr => pr.PersonId)
                .ToListAsync(ct);

            if (linkedPersonIds.Count == 0)
            {
                await _auditLogger.LogAsync(new AccessAuditEntry
                {
                    UserId = userId.Value,
                    Role = role,
                    AccessedPersonId = null,
                    ActionType = mode == AccessMode.Write ? AccessAuditValues.Action.Update : AccessAuditValues.Action.Read,
                    Result = AccessAuditValues.Result.Denied,
                    AffectedTable = "FamilyRepresentatives",
                    AffectedRecordId = familyId.ToString()
                }, ct);
                return false;
            }

            // Check if caller can access any linked person
            var accessibleIds = await GetAccessiblePersonIdsAsync(ct);
            var allowed = linkedPersonIds.Any(id => accessibleIds.Contains(id));

            await _auditLogger.LogAsync(new AccessAuditEntry
            {
                UserId = userId.Value,
                Role = role,
                AccessedPersonId = null,
                ActionType = mode == AccessMode.Write ? AccessAuditValues.Action.Update : AccessAuditValues.Action.Read,
                Result = allowed ? AccessAuditValues.Result.Allowed : AccessAuditValues.Result.Denied,
                AffectedTable = "FamilyRepresentatives",
                AffectedRecordId = familyId.ToString()
            }, ct);

            return allowed;
        }

        public async Task<bool> CanAccessReportAsync(int reportId, AccessMode mode, CancellationToken ct = default)
        {
            var report = await _context.Reports
                .Where(r => r.Id == reportId)
                .Select(r => new { r.PersonId, r.ProfessionalId })
                .FirstOrDefaultAsync(ct);

            if (report is null)
                return false;

            // El profesional siempre puede leer sus propios reportes aunque ya no tenga
            // un assignment activo con esa persona (el reporte es evidencia histórica).
            if (mode == AccessMode.Read)
            {
                var entityId = _httpContext.GetCurrentEntityId();
                if (entityId.HasValue && report.ProfessionalId == entityId.Value)
                    return true;
            }

            return await CanAccessPersonAsync(report.PersonId, mode, ct);
        }

        public async Task<bool> CanAccessDiagnosisAsync(int diagnosisId, AccessMode mode, CancellationToken ct = default)
        {
            var personId = await _context.Diagnoses
                .Where(d => d.Id == diagnosisId)
                .Select(d => (Guid?)d.PersonId)
                .FirstOrDefaultAsync(ct);

            if (personId is null)
            {
                return false;
            }

            return await CanAccessPersonAsync(personId.Value, mode, ct);
        }

        public Task<bool> CanAccessSkillProfileAsync(Guid personId, AccessMode mode, CancellationToken ct = default)
            => CanAccessPersonAsync(personId, mode, ct); // SkillProfile 1:1 con Person, misma regla

        public Task<bool> CanAccessResponseAsync(Guid responseId, AccessMode mode, CancellationToken ct = default)
            => throw new NotImplementedException("Pendiente: Fase 2 al implementar BE-11");

        public Task<bool> CanAccessActivityAssignmentAsync(Guid assignmentId, AccessMode mode, CancellationToken ct = default)
            => throw new NotImplementedException("Pendiente: Fase 2 al implementar BE-10");

        public Task<bool> CanAccessRoadmapAsync(Guid personId, AccessMode mode, CancellationToken ct = default)
            => CanAccessPersonAsync(personId, mode, ct); // Roadmap 1:1 con Person, misma regla

        public async Task<bool> CanAccessInvitationAsync(int invitationId, AccessMode mode, CancellationToken ct = default)
        {
            var invitation = await _context.Invitations
                .Where(i => i.Id == invitationId)
                .Select(i => new { i.ForPersonId, i.CreatedByProfessionalId })
                .FirstOrDefaultAsync(ct);

            if (invitation is null)
            {
                return false;
            }

            // Si la invitacion apunta a una persona, usar la regla canonica de persona.
            if (invitation.ForPersonId.HasValue)
            {
                return await CanAccessPersonAsync(invitation.ForPersonId.Value, mode, ct);
            }

            // Sin persona asociada: solo el profesional creador o GlobalAdmin.
            if (_httpContext.IsGlobalAdmin())
            {
                return true;
            }

            // El professionalId viene del JWT — sin consulta adicional a BD.
            var currentProfessionalId = _httpContext.GetCurrentEntityId();

            return currentProfessionalId.HasValue
                && currentProfessionalId.Value == invitation.CreatedByProfessionalId;
        }

        public async Task<bool> CanAccessUserAsync(Guid targetUserId, AccessMode mode, CancellationToken ct = default)
        {
            var currentUserId = _httpContext.GetCurrentUserId();
            if (currentUserId is null)
            {
                return false;
            }

            // Self-access siempre permitido (el propio perfil).
            if (currentUserId.Value == targetUserId)
            {
                return true;
            }

            if (_httpContext.IsGlobalAdmin())
            {
                return true;
            }

            // Si el target es una PersonWithDisability, delegar al check canonico.
            var personId = await _context.PersonsWithDisability
                .Where(p => p.UserId == targetUserId)
                .Select(p => (Guid?)p.Id)
                .FirstOrDefaultAsync(ct);

            if (personId.HasValue)
            {
                return await CanAccessPersonAsync(personId.Value, mode, ct);
            }

            // Target es otro tipo de usuario (profesional, familiar, admin):
            // sin regla explicita en Fase 2 → denegar. Se extiende en Fase 3.
            return false;
        }

        public async Task<bool> CanSuperviseLoginAsync(Guid personId, CancellationToken ct = default)
        {
            var userId = _httpContext.GetCurrentUserId();
            if (userId is null)
            {
                return false;
            }

            if (_httpContext.IsGlobalAdmin())
            {
                return true;
            }

            var role = _httpContext.GetCurrentUserRole();
            var entityId = _httpContext.GetCurrentEntityId();

            if (role == nameof(IdentityRoles.Professional))
            {
                // Preferir entityId del JWT; fallback a UserId join si no está disponible.
                return entityId.HasValue
                    ? await _context.ProfessionalPersons
                        .AnyAsync(pp => pp.ProfessionalId == entityId.Value
                                     && pp.PersonId == personId
                                     && pp.IsActive
                                     && pp.CanSuperviseLogin, ct)
                    : await _context.ProfessionalPersons
                        .AnyAsync(pp => pp.Professional.UserId == userId.Value
                                     && pp.PersonId == personId
                                     && pp.IsActive
                                     && pp.CanSuperviseLogin, ct);
            }

            if (role == nameof(IdentityRoles.FamilyRepresentative))
            {
                // Preferir entityId del JWT; fallback a UserId join si no está disponible.
                return entityId.HasValue
                    ? await _context.PersonRepresentatives
                        .AnyAsync(pr => pr.RepresentativeId == entityId.Value
                                     && pr.PersonId == personId
                                     && pr.IsActive
                                     && pr.CanSuperviseLogin, ct)
                    : await _context.PersonRepresentatives
                        .AnyAsync(pr => pr.Representative.UserId == userId.Value
                                     && pr.PersonId == personId
                                     && pr.IsActive
                                     && pr.CanSuperviseLogin, ct);
            }

            return false;
        }
    }
}
