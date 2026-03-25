using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminUsers.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.AdminUsers.Handlers
{
    public class AdminDeactivateUserCommandHandler : ICommandHandler<AdminDeactivateUserCommand, ApiResponse<object>>
    {
        private readonly IIdentityService _identityService;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AdminDeactivateUserCommandHandler> _logger;

        public AdminDeactivateUserCommandHandler(
            IIdentityService identityService,
            IRefreshTokensRepository refreshTokensRepository,
            IProfessionalsRepository professionalsRepository,
            IPersonsRepository personsRepository,
            IFamilyRepository familyRepository,
            IUnitOfWork unitOfWork,
            ILogger<AdminDeactivateUserCommandHandler> logger)
        {
            _identityService = identityService;
            _refreshTokensRepository = refreshTokensRepository;
            _professionalsRepository = professionalsRepository;
            _personsRepository = personsRepository;
            _familyRepository = familyRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<object>> HandleAsync(
            AdminDeactivateUserCommand command, CancellationToken cancellationToken)
        {
            if (command.UserId == command.RequestedByUserId)
            {
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.CannotDeactivateSelf,
                    "No puede desactivar su propia cuenta.");
            }

            var user = await _identityService.FindByIdAsync(command.UserId);
            if (user is null)
                return ApiResponse<object>.NotFound("Usuario");

            if (!user.IsActive)
            {
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.UserAlreadyInactive,
                    "El usuario ya se encuentra inactivo.");
            }

            user.IsActive = false;
            await _identityService.UpdateUserAsync(user);

            await _refreshTokensRepository.RevokeAllUserTokensAsync(
                user.Id, Constants.RevokeReasons.UserDeactivated, cancellationToken);

            // Desactivar entidad vinculada
            var roles = await _identityService.GetRolesAsync(user);
            var primaryRole = roles.FirstOrDefault();

            switch (primaryRole)
            {
                case "Professional":
                    var pro = await _professionalsRepository.GetByUserIdAsync(user.Id, cancellationToken);
                    if (pro is not null)
                    {
                        pro.User.IsActive = false;
                        await _professionalsRepository.UpdateAsync(pro, cancellationToken);
                    }
                    break;

                case "PersonWithDisability":
                    var person = await _personsRepository.GetByUserIdAsync(user.Id, cancellationToken);
                    if (person is not null)
                    {
                        person.User.IsActive = false;
                        await _personsRepository.UpdateAsync(person, cancellationToken);
                    }
                    break;

                case "FamilyRepresentative":
                    var family = await _familyRepository.GetByUserIdAsync(user.Id, cancellationToken);
                    if (family is not null)
                    {
                        family.User.IsActive = false;
                        await _familyRepository.UpdateAsync(family, cancellationToken);
                    }
                    break;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {UserId} ({Email}) deactivated by admin {AdminId}",
                user.Id, user.Email, command.RequestedByUserId);

            return ApiResponse<object>.SuccessResult("Usuario desactivado exitosamente.");
        }
    }
}
