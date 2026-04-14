using Microsoft.Extensions.Logging;
using InclusiON.Application.Helpers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Professionals.Handlers
{
    /// <summary>
    /// Handler para el registro público de profesionales.
    /// El profesional se registra con estado Pending y SIN usuario.
    /// El usuario se crea cuando un admin lo valida.
    /// </summary>
    public class RegisterProfessionalCommandHandler : ICommandHandler<RegisterProfessionalCommand, ApiResponse<ProfessionalResponse>>
    {
        private readonly IProfessionalsRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RegisterProfessionalCommandHandler> _logger;

        public RegisterProfessionalCommandHandler(
            IProfessionalsRepository repository,
            IIdentityService identityService,
            IEmailService emailService,
            IUnitOfWork unitOfWork,
            ILogger<RegisterProfessionalCommandHandler> logger)
        {
            _repository = repository;
            _identityService = identityService;
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<ProfessionalResponse>> HandleAsync(RegisterProfessionalCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(command.DocumentNumber))
                {
                    var documentExists = await _repository.ExistsDocumentAsync(command.DocumentNumber, null, cancellationToken);
                    if (documentExists)
                    {
                        return ApiResponse<ProfessionalResponse>.Conflict(
                            ErrorCode.DocumentAlreadyExists,
                            ErrorMessages.DocumentAlreadyExists);
                    }
                }

                var existingUser = await _identityService.FindByEmailAsync(command.Email);
                if (existingUser != null)
                {
                    return ApiResponse<ProfessionalResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        ErrorMessages.EmailAlreadyRegistered);
                }

                var userId = Guid.NewGuid();
                var tempPassword = PasswordGenerator.GenerateTemporary();

                var user = new User
                {
                    Id = userId,
                    Name = command.FirstName,
                    Surname = command.LastName,
                    Email = command.Email,
                    UserName = command.Email,
                    EmailConfirmed = false,
                    IsActive = false,
                    CreatedAt = DateTime.UtcNow
                };

                var createUserResult = await _identityService.CreateUserAsync(user, tempPassword);
                if (!createUserResult.Succeeded)
                {
                    var errors = string.Join(", ", createUserResult.Errors);
                    _logger.LogError("Error creando usuario para profesional: {Errors}", errors);
                    return ApiResponse<ProfessionalResponse>.ErrorResult(
                        ErrorCode.InternalError,
                        $"Error al crear usuario: {errors}");
                }

                await _identityService.AddToRoleAsync(user, IdentityRoles.Professional.ToString());

                var professional = new Professional
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    DocumentNumber = command.DocumentNumber,
                    Phone = command.Phone,
                    Specialty = command.Specialty,
                    LicenseNumber = command.LicenseNumber,
                    BirthDate = command.BirthDate,
                    Email = command.Email,
                    Status = ProfessionalStatusEnum.Pending,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                if (command.InstitutionId.HasValue)
                {
                    professional.ProfessionalInstitutions.Add(new ProfessionalInstitution
                    {
                        ProfessionalId = professional.Id,
                        InstitutionId = command.InstitutionId.Value,
                        AssignedAt = DateTime.UtcNow,
                        IsActive = true
                    });
                }

                await _repository.CreateAsync(professional, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Profesional registrado (pendiente): {ProfessionalId}, UserId: {UserId}, Email: {Email}", 
                    professional.Id, userId, command.Email);

                // TODO: Refactorizar usando Microsoft.Extensions.AI / Semantic Kernel Agent Framework
                // para orquestar notificaciones de forma inteligente (reintentos, canales múltiples, prioridad).
                try
                {
                    await _emailService.SendTemplatedEmailAsync(
                        command.Email,
                        "Tu registro en InclusiON está pendiente de validación",
                        "ProfessionalPendingRegistration",
                        new Dictionary<string, string?>
                        {
                            { "FirstName", command.FirstName },
                            { "LastName", command.LastName },
                            { "Year", DateTime.UtcNow.Year.ToString() }
                        },
                        cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo enviar email de registro pendiente a {Email}", command.Email);
                }

                var response = ProfessionalResponse.MapToResponse(professional);
                return ApiResponse<ProfessionalResponse>.SuccessResult(response, SuccessMessages.ProfessionalPendingApproval);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al registrar profesional: {Email}", command.Email);
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    ErrorMessages.InternalErrorRegister);
            }
        }
    }
}