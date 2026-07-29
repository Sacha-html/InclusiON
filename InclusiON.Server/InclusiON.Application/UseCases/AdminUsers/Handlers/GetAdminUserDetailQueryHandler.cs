using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.AdminUsers.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class GetAdminUserDetailQueryHandler : IQueryHandler<GetAdminUserDetailQuery, ApiResponse<AdminUserDetailResponse>>
    {
        private readonly IIdentityService              _identityService;
        private readonly IProfessionalsRepository      _professionalsRepository;
        private readonly IPersonsRepository            _personsRepository;
        private readonly IFamilyRepository             _familyRepository;
        private readonly IAdminInstitutionRepository   _adminInstitutionRepository;
        private readonly IAssignmentsRepository        _assignmentsRepository;

        public GetAdminUserDetailQueryHandler(
            IIdentityService              identityService,
            IProfessionalsRepository      professionalsRepository,
            IPersonsRepository            personsRepository,
            IFamilyRepository             familyRepository,
            IAdminInstitutionRepository   adminInstitutionRepository,
            IAssignmentsRepository        assignmentsRepository)
        {
            _identityService              = identityService;
            _professionalsRepository      = professionalsRepository;
            _personsRepository            = personsRepository;
            _familyRepository             = familyRepository;
            _adminInstitutionRepository   = adminInstitutionRepository;
            _assignmentsRepository        = assignmentsRepository;
        }

        public async Task<ApiResponse<AdminUserDetailResponse>> HandleAsync(
            GetAdminUserDetailQuery query, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByIdAsync(query.UserId);
            if (user is null)
                return ApiResponse<AdminUserDetailResponse>.NotFound("Usuario");

            // Validación de alcance institucional
            if (query.InstitutionIds is { Count: > 0 })
            {
                var targetInstitutions = await _adminInstitutionRepository
                    .GetActiveInstitutionIdsByAdminAsync(query.UserId, cancellationToken);

                if (targetInstitutions.Count > 0)
                {
                    var hasOverlap = targetInstitutions.Any(id => query.InstitutionIds!.Contains(id));
                    if (!hasOverlap)
                        return ApiResponse<AdminUserDetailResponse>.Forbidden(
                            "No tiene permisos para ver detalles de un usuario de otra institución.");
                }
            }

            // DbContext is not thread-safe — sequential queries with short-circuit.
            var roles       = await _identityService.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Unknown";

            var (entityType, linkedEntity) = await LoadLinkedEntityAsync(user.Id, cancellationToken);

            return ApiResponse<AdminUserDetailResponse>.SuccessResult(
                AdminUserMapper.ToAdminUserDetailResponse(user, entityType, linkedEntity, primaryRole));
        }

        private async Task<(string? EntityType, AdminUserMapper.LinkedEntityData? Data)> LoadLinkedEntityAsync(
            Guid userId, CancellationToken cancellationToken)
        {
            // Sequential with short-circuit — most users have exactly one linked entity type.
            if (await _professionalsRepository.GetByUserIdAsync(userId, cancellationToken) is { } pro)
                return (RoleNames.Professional,
                    new AdminUserMapper.LinkedEntityData(pro.Id, pro.FirstName, pro.LastName,
                        pro.Specialty, pro.LicenseNumber, pro.DocumentNumber, pro.Phone, null));

            if (await _personsRepository.GetByUserIdAsync(userId, cancellationToken) is { } person)
            {
                var professionals = await _assignmentsRepository.GetProfessionalsByPersonIdAsync(person.Id, cancellationToken);
                var activePros = (professionals ?? []).Where(p => p.IsActive).Select(p => $"{p.Professional?.FirstName} {p.Professional?.LastName}".Trim());
                var supervisorName = activePros.Any() ? string.Join(", ", activePros) : "Ninguno";

                var representatives = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(person.Id, cancellationToken);
                var activeReps = (representatives ?? []).Where(r => r.IsActive).Select(r => $"{r.Representative?.FirstName} {r.Representative?.LastName}".Trim());
                var representativeName = activeReps.Any() ? string.Join(", ", activeReps) : "Ninguno";

                return (RoleNames.PersonWithDisability,
                    new AdminUserMapper.LinkedEntityData(person.Id, person.FirstName, person.LastName,
                        null, null, person.DocumentNumber, null, null, supervisorName, representativeName));
            }

            if (await _familyRepository.GetByUserIdAsync(userId, cancellationToken) is { } family)
                return (RoleNames.FamilyRepresentative,
                    new AdminUserMapper.LinkedEntityData(family.Id, family.FirstName, family.LastName,
                        null, null, family.DocumentNumber, family.Phone, family.Relationship));

            return (null, null);
        }
    }
}
