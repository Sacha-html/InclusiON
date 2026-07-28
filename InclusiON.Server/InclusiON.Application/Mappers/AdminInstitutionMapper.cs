using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.Mappers
{
    public static class AdminInstitutionMapper
    {
        public static AdminInstitutionResponse ToResponse(AdminInstitution ai) => new()
        {
            AdminUserId     = ai.AdminUserId,
            InstitutionId   = ai.InstitutionId,
            InstitutionName = ai.Institution.Name,
            AssignedAt      = ai.AssignedAt,
            IsActive        = ai.IsActive,
        };

        /// <summary>
        /// Overload para cuando la navigation property Institution no está cargada
        /// y el nombre se pasa explícitamente (ej: AssignInstitutionToAdminCommandHandler).
        /// </summary>
        public static AdminInstitutionResponse ToResponse(AdminInstitution ai, string institutionName) => new()
        {
            AdminUserId     = ai.AdminUserId,
            InstitutionId   = ai.InstitutionId,
            InstitutionName = institutionName,
            AssignedAt      = ai.AssignedAt,
            IsActive        = ai.IsActive,
        };

        public static AdminUserResponse ToAdminUserResponse(User u) => new()
        {
            Id            = u.Id,
            Name          = u.Name ?? string.Empty,
            Surname       = u.Surname ?? string.Empty,
            Email         = u.Email!,
            IsActive      = u.IsActive,
            CreatedAt     = u.CreatedAt,
            IsGlobalAdmin = !u.AdminInstitutions.Any(),
            Institutions  = u.AdminInstitutions.Select(ai => new AdminInstitutionInfo
            {
                InstitutionId   = ai.InstitutionId,
                InstitutionName = ai.Institution.Name,
            }).ToList(),
        };
    }
}
