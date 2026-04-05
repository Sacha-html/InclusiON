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

            var rolesTask = _identityService.GetRolesAsync(user);
            var linkedEntityTask = LoadLinkedEntityAsync(user.Id, cancellationToken);
            
            await Task.WhenAll(rolesTask, linkedEntityTask);
            
            var roles = await rolesTask;
            var (entityType, linkedEntity) = await linkedEntityTask;
            var primaryRole = roles.FirstOrDefault() ?? "Unknown";

            var response = new AdminUserDetailResponse
            {
                UserId = user.Id,
                Email = user.Email ?? string.Empty,
                Name = user.Name,
                Surname = user.Surname,
                FullName = (entityType, linkedEntity) switch
                {
                    ("Professional", { }) => $"{linkedEntity.FirstName} {linkedEntity.LastName}",
                    ("PersonWithDisability", { }) => $"{linkedEntity.FirstName} {linkedEntity.LastName}",
                    ("FamilyRepresentative", { }) => $"{linkedEntity.FirstName} {linkedEntity.LastName}",
                    _ => $"{user.Name} {user.Surname}".Trim()
                },
                Role = primaryRole,
                IsActive = user.IsActive,
                LastLoginDate = user.LastLoginDate,
                LastLoginIpAddress = user.LastLoginIpAddress,
                CreatedAt = user.CreatedAt,
                MustChangePassword = user.MustChangePassword,
                LinkedEntity = (entityType, linkedEntity) switch
                {
                    ("Professional", { } e) => new LinkedEntityInfo
                    {
                        EntityType = entityType,
                        EntityId = e.Id,
                        Specialty = e.Specialty,
                        LicenseNumber = e.LicenseNumber,
                        DocumentNumber = e.DocumentNumber,
                        Phone = e.Phone
                    },
                    ("PersonWithDisability", { } e) => new LinkedEntityInfo
                    {
                        EntityType = entityType,
                        EntityId = e.Id,
                        DocumentNumber = e.DocumentNumber
                    },
                    ("FamilyRepresentative", { } e) => new LinkedEntityInfo
                    {
                        EntityType = entityType,
                        EntityId = e.Id,
                        DocumentNumber = e.DocumentNumber,
                        Phone = e.Phone,
                        Relationship = e.Relationship
                    },
                    _ => null
                }
            };

            return ApiResponse<AdminUserDetailResponse>.SuccessResult(response);
        }

        private async Task<(string? EntityType, LinkedEntityData?)> LoadLinkedEntityAsync(
            Guid userId, CancellationToken cancellationToken)
        {
            var proTask = _professionalsRepository.GetByUserIdAsync(userId, cancellationToken);
            var personTask = _personsRepository.GetByUserIdAsync(userId, cancellationToken);
            var familyTask = _familyRepository.GetByUserIdAsync(userId, cancellationToken);

            await Task.WhenAll(proTask, personTask, familyTask);

            if (await proTask is { } pro)
                return ("Professional", new LinkedEntityData(pro.Id, pro.FirstName, pro.LastName, pro.Specialty, pro.LicenseNumber, pro.DocumentNumber, pro.Phone, null));

            if (await personTask is { } person)
                return ("PersonWithDisability", new LinkedEntityData(person.Id, person.FirstName, person.LastName, null, null, person.DocumentNumber, null, null));

            if (await familyTask is { } family)
                return ("FamilyRepresentative", new LinkedEntityData(family.Id, family.FirstName, family.LastName, null, null, family.DocumentNumber, family.Phone, family.Relationship));

            return (null, null);
        }

        private record LinkedEntityData(Guid Id, string FirstName, string LastName, string? Specialty, string? LicenseNumber, string? DocumentNumber, string? Phone, string? Relationship);
    }
}
