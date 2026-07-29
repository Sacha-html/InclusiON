using InclusiON.Application.Constants;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.Mappers
{
    public static class AdminUserMapper
    {
        public static UserRecentSessionResponse ToSessionResponse(RefreshToken t) => new()
        {
            CreatedAt     = t.CreatedAt,
            IpAddress     = t.CreatedByIp,
            UserAgent     = t.UserAgent,
            IsActive      = t.IsActive,
            ExpiresAt     = t.ExpiresAt,
            RevokedAt     = t.RevokedAt,
            RevokedReason = t.RevokedReason,
        };

        public static AdminUserDetailResponse ToAdminUserDetailResponse(
            User user, string? entityType, LinkedEntityData? linked, string primaryRole) => new()
        {
            UserId             = user.Id,
            Email              = user.Email ?? string.Empty,
            Name               = user.Name,
            Surname            = user.Surname,
            FullName           = linked is not null
                                    ? $"{linked.FirstName} {linked.LastName}".Trim()
                                    : $"{user.Name} {user.Surname}".Trim(),
            Role               = primaryRole,
            IsActive           = user.IsActive,
            LastLoginDate      = user.LastLoginDate,
            LastLoginIpAddress = user.LastLoginIpAddress,
            CreatedAt          = user.CreatedAt,
            MustChangePassword = user.MustChangePassword,
            LinkedEntity = (entityType, linked) switch
            {
                (RoleNames.Professional, { } e) => new LinkedEntityInfo
                {
                    EntityType     = entityType,
                    EntityId       = e.Id,
                    Specialty      = e.Specialty,
                    LicenseNumber  = e.LicenseNumber,
                    DocumentNumber = e.DocumentNumber,
                    Phone          = e.Phone
                },
                (RoleNames.PersonWithDisability, { } e) => new LinkedEntityInfo
                {
                    EntityType     = entityType,
                    EntityId       = e.Id,
                    DocumentNumber = e.DocumentNumber,
                    SupervisorName = e.SupervisorName,
                    RepresentativeName = e.RepresentativeName
                },
                (RoleNames.FamilyRepresentative, { } e) => new LinkedEntityInfo
                {
                    EntityType     = entityType,
                    EntityId       = e.Id,
                    DocumentNumber = e.DocumentNumber,
                    Phone          = e.Phone,
                    Relationship   = e.Relationship
                },
                _ => null
            }
        };

        /// <summary>
        /// Intermediate data carrier for linked entity information loaded from DB.
        /// Used by GetAdminUserDetailQueryHandler to pass loaded entity data to the mapper.
        /// </summary>
        public record LinkedEntityData(
            Guid    Id,
            string  FirstName,
            string  LastName,
            string? Specialty,
            string? LicenseNumber,
            string? DocumentNumber,
            string? Phone,
            string? Relationship,
            string? SupervisorName = null,
            string? RepresentativeName = null);
    }
}
