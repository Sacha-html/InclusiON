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
        private readonly IIdentityService        _identityService;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IPersonsRepository      _personsRepository;
        private readonly IFamilyRepository       _familyRepository;

        public GetAdminUserDetailQueryHandler(
            IIdentityService        identityService,
            IProfessionalsRepository professionalsRepository,
            IPersonsRepository      personsRepository,
            IFamilyRepository       familyRepository)
        {
            _identityService         = identityService;
            _professionalsRepository = professionalsRepository;
            _personsRepository       = personsRepository;
            _familyRepository        = familyRepository;
        }

        public async Task<ApiResponse<AdminUserDetailResponse>> HandleAsync(
            GetAdminUserDetailQuery query, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByIdAsync(query.UserId);
            if (user is null)
                return ApiResponse<AdminUserDetailResponse>.NotFound("Usuario");

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
                return (RoleNames.PersonWithDisability,
                    new AdminUserMapper.LinkedEntityData(person.Id, person.FirstName, person.LastName,
                        null, null, person.DocumentNumber, null, null));

            if (await _familyRepository.GetByUserIdAsync(userId, cancellationToken) is { } family)
                return (RoleNames.FamilyRepresentative,
                    new AdminUserMapper.LinkedEntityData(family.Id, family.FirstName, family.LastName,
                        null, null, family.DocumentNumber, family.Phone, family.Relationship));

            return (null, null);
        }
    }
}
