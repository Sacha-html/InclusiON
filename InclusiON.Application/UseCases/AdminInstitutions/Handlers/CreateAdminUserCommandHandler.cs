using Microsoft.Extensions.Logging;
using InclusiON.Application.Constants;
using InclusiON.Application.Helpers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminInstitutions.Commands;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Admin;

namespace InclusiON.Application.UseCases.AdminInstitutions.Handlers
{
    public class CreateAdminUserCommandHandler
        : ICommandHandler<CreateAdminUserCommand, ApiResponse<CreateAdminUserResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly IInstitutionsRepository _institutionsRepository;
        private readonly IAdminInstitutionRepository _adminInstitutionRepository;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly ILogger<CreateAdminUserCommandHandler> _logger;

        public CreateAdminUserCommandHandler(
            IIdentityService identityService,
            IInstitutionsRepository institutionsRepository,
            IAdminInstitutionRepository adminInstitutionRepository,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            ILogger<CreateAdminUserCommandHandler> logger)
        {
            _identityService            = identityService;
            _institutionsRepository     = institutionsRepository;
            _adminInstitutionRepository = adminInstitutionRepository;
            _emailService               = emailService;
            _unitOfWork                 = unitOfWork;
            _dateTime                   = dateTime;
            _logger                     = logger;
        }

        public async Task<ApiResponse<CreateAdminUserResponse>> HandleAsync(
            CreateAdminUserCommand command, CancellationToken cancellationToken)
        {
            var institution = await _institutionsRepository.GetByIdAsync(command.InstitutionId, cancellationToken);
            if (institution is null)
                return ApiResponse<CreateAdminUserResponse>.NotFound("Institución");

            var existingUser = await _identityService.FindByEmailAsync(command.Email);
            if (existingUser is not null)
                return ApiResponse<CreateAdminUserResponse>.Conflict(
                    ErrorCode.EmailAlreadyExists, "Ya existe un usuario con ese email.");

            var tempPassword = PasswordGenerator.GenerateTemporary();

            var user = new User
            {
                Name                = command.FirstName,
                Surname             = command.LastName,
                Email               = command.Email.ToLower(),
                UserName            = command.Email.ToLower(),
                NormalizedEmail     = command.Email.ToUpper(),
                NormalizedUserName  = command.Email.ToUpper(),
                EmailConfirmed      = true,
                IsActive            = true,
                MustChangePassword  = true,
                CreatedAt           = _dateTime.UtcNow
            };

            var createResult = await _identityService.CreateUserAsync(user, tempPassword);
            if (!createResult.Succeeded)
                return ApiResponse<CreateAdminUserResponse>.ErrorResult(
                    "Error al crear el usuario.", createResult.Errors.ToList());

            var roleResult = await _identityService.AddToRoleAsync(user, RoleNames.Admin);
            if (!roleResult.Succeeded)
                return ApiResponse<CreateAdminUserResponse>.ErrorResult(
                    "Error al asignar el rol.", roleResult.Errors.ToList());

            await _adminInstitutionRepository.AddAsync(new AdminInstitution
            {
                AdminUserId   = user.Id,
                InstitutionId = command.InstitutionId,
                AssignedAt    = _dateTime.UtcNow,
                IsActive      = true
            }, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Enviar credenciales por email — la contraseña temporal nunca viaja en la respuesta HTTP.
            try
            {
                await _emailService.SendTemplatedEmailAsync(
                    user.Email!,
                    "Bienvenido a InclusiON — Tus credenciales de acceso",
                    "PasswordReset",
                    new Dictionary<string, string?>
                    {
                        { "UserName",          user.Name ?? "Administrador" },
                        { "TemporaryPassword", tempPassword },
                        { "Year",              _dateTime.UtcNow.Year.ToString() }
                    },
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo enviar email de bienvenida a {UserId}", user.Id);
            }

            return ApiResponse<CreateAdminUserResponse>.SuccessResult(new CreateAdminUserResponse
            {
                UserId          = user.Id,
                Email           = user.Email!,
                FirstName       = command.FirstName,
                LastName        = command.LastName,
                InstitutionId   = command.InstitutionId,
                InstitutionName = institution.Name
            }, "Usuario administrador creado exitosamente. Se enviaron las credenciales por email.");
        }
    }
}
