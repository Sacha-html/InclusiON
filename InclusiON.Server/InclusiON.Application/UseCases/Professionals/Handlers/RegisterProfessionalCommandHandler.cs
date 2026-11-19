using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Helpers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;
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
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RegisterProfessionalCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IReadOnlyRepository<Specialty>? _specialties;
        private readonly IAdminInstitutionRepository? _adminInstitutionRepository;
        private readonly INotificationService? _notificationService;

        public RegisterProfessionalCommandHandler(
            IProfessionalsRepository repository,
            IIdentityService identityService,
            IBackgroundJobRepository backgroundJobs,
            IUnitOfWork unitOfWork,
            ILogger<RegisterProfessionalCommandHandler> logger,
            IDateTimeProvider dateTime,
            IReadOnlyRepository<Specialty>? specialties = null,
            IAdminInstitutionRepository? adminInstitutionRepository = null,
            INotificationService? notificationService = null)
        {
            _repository                 = repository;
            _identityService            = identityService;
            _backgroundJobs             = backgroundJobs;
            _unitOfWork                 = unitOfWork;
            _logger                     = logger;
            _dateTime                   = dateTime;
            _specialties                = specialties;
            _adminInstitutionRepository = adminInstitutionRepository;
            _notificationService        = notificationService;
        }

        public async Task<ApiResponse<ProfessionalResponse>> HandleAsync(RegisterProfessionalCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (command.SpecialtyId.HasValue && _specialties is not null && (await _specialties.GetByIdAsync(command.SpecialtyId.Value, cancellationToken))?.IsActive != true)
                    return ApiResponse<ProfessionalResponse>.ErrorResult(ErrorCode.ValidationFailed, "La especialidad seleccionada no está activa.");
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
                    CreatedAt = _dateTime.UtcNow
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
                    SpecialtyId = command.SpecialtyId,
                    LicenseNumber = command.LicenseNumber,
                    BirthDate = command.BirthDate,
                    Email = command.Email,
                    Status = ProfessionalStatusEnum.Pending,
                    IsActive = true,
                    CreatedAt = _dateTime.UtcNow
                };

                if (command.InstitutionId.HasValue)
                {
                    professional.ProfessionalInstitutions.Add(new ProfessionalInstitution
                    {
                        ProfessionalId = professional.Id,
                        InstitutionId = command.InstitutionId.Value,
                        AssignedAt = _dateTime.UtcNow,
                        IsActive = true
                    });
                }

                await _repository.CreateAsync(professional, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Profesional registrado (pendiente): {ProfessionalId}, UserId: {UserId}, Email: {Email}", 
                    professional.Id, userId, command.Email);

                await _backgroundJobs.CreateAsync(
                    JobTypes.Email,
                    JsonSerializer.Serialize(new EmailPayload
                    {
                        To           = command.Email,
                        Subject      = "Tu registro en InclusiON está pendiente de validación",
                        TemplateName = "ProfessionalPendingRegistration",
                        Replacements = new Dictionary<string, string?>
                        {
                            { "FirstName", command.FirstName },
                            { "LastName", command.LastName },
                            { "Year", _dateTime.UtcNow.Year.ToString() }
                        }
                    }),
                    maxRetries: 2,
                    cancellationToken: cancellationToken);

                // Notificar a los administradores de la institución y admins globales
                if (_notificationService is not null && _adminInstitutionRepository is not null)
                {
                    var instIds = command.InstitutionId.HasValue ? new List<int> { command.InstitutionId.Value } : null;
                    var admins = await _adminInstitutionRepository.GetAdminsForInstitutionsIncludingGlobalAsync(instIds, cancellationToken);
                    var notifications = admins
                        .Where(a => a.IsActive)
                        .Select(admin => new NotificationMessage(
                            UserId: admin.Id,
                            Type: NotificationTypes.System,
                            EventCode: NotificationEventCodes.ProfessionalRegistered,
                            Title: "Nuevo profesional pendiente de validación",
                            Message: $"{command.FirstName} {command.LastName} ({command.Email}) se registró y aguarda validación.",
                            ActionUrl: "/admin/professionals"));

                    await _notificationService.NotifyManyAsync(notifications, cancellationToken);
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
