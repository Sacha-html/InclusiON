using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminUsers.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class GetAdminUserDetailQueryHandler : IQueryHandler<GetAdminUserDetailQuery, ApiResponse<AdminUserDetailResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IFamilyRepository _familyRepository;

        public GetAdminUserDetailQueryHandler(
            IIdentityService identityService,
            IProfessionalsRepository professionalsRepository,
            IPersonsRepository personsRepository,
            IFamilyRepository familyRepository)
        {
            _identityService = identityService;
            _professionalsRepository = professionalsRepository;
            _personsRepository = personsRepository;
            _familyRepository = familyRepository;
        }

        public async Task<ApiResponse<AdminUserDetailResponse>> HandleAsync(
            GetAdminUserDetailQuery query, CancellationToken cancellationToken)
        {
            var user = await _identityService.FindByIdAsync(query.UserId);
            if (user is null)
                return ApiResponse<AdminUserDetailResponse>.NotFound("Usuario");

            var roles = await _identityService.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault() ?? "Unknown";

            var response = new AdminUserDetailResponse
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Name = user.Name,
                Surname = user.Surname,
                FullName = $"{user.Name} {user.Surname}".Trim(),
                Role = primaryRole,
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                LastLoginIpAddress = user.LastLoginIpAddress,
                CreatedAt = user.CreatedAt,
                MustChangePassword = user.MustChangePassword
            };

            // Cargar entidad vinculada segun el rol
            switch (primaryRole)
            {
                case "Professional":
                    var pro = await _professionalsRepository.GetByUserIdAsync(user.Id, cancellationToken);
                    if (pro is not null)
                    {
                        response.FullName = $"{pro.FirstName} {pro.LastName}".Trim();
                        response.LinkedEntity = new LinkedEntityInfo
                        {
                            EntityType = "Professional",
                            EntityId = pro.Id,
                            Specialty = pro.Specialty,
                            LicenseNumber = pro.LicenseNumber,
                            DocumentNumber = pro.DocumentNumber,
                            Phone = pro.Phone
                        };
                    }
                    break;

                case "PersonWithDisability":
                    var person = await _personsRepository.GetByUserIdAsync(user.Id, cancellationToken);
                    if (person is not null)
                    {
                        response.FullName = $"{person.FirstName} {person.LastName}".Trim();
                        response.LinkedEntity = new LinkedEntityInfo
                        {
                            EntityType = "PersonWithDisability",
                            EntityId = person.Id,
                            DocumentNumber = person.DocumentNumber
                        };
                    }
                    break;

                case "FamilyRepresentative":
                    var family = await _familyRepository.GetByUserIdAsync(user.Id, cancellationToken);
                    if (family is not null)
                    {
                        response.FullName = $"{family.FirstName} {family.LastName}".Trim();
                        response.LinkedEntity = new LinkedEntityInfo
                        {
                            EntityType = "FamilyRepresentative",
                            EntityId = family.Id,
                            DocumentNumber = family.DocumentNumber,
                            Phone = family.Phone,
                            Relationship = family.Relationship
                        };
                    }
                    break;

                case "Admin":
                    response.LinkedEntity = new LinkedEntityInfo
                    {
                        EntityType = "Admin"
                    };
                    break;
            }

            return ApiResponse<AdminUserDetailResponse>.SuccessResult(response);
        }
    }
}
