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
        private readonly IUniquenessValidationService _uniquenessValidation;
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
            IUniquenessValidationService uniquenessValidation,
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
            _uniquenessValidation       = uniquenessValidation;
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

                var normalizedDoc = UniquenessNormalizer.NormalizeDocument(command.DocumentNumber);
                var normalizedEmail = UniquenessNormalizer.NormalizeEmail(command.Email) ?? string.Empty;
                var normalizedLicense = UniquenessNormalizer.NormalizeLicense(command.LicenseNumber);

                if (normalizedDoc is not null)
                {
                    var docCheck = await _uniquenessValidation.CheckDocumentAsync(normalizedDoc, null, cancellationToken);
                    if (!docCheck.IsAvailable)
                    {
                        return ApiResponse<ProfessionalResponse>.Conflict(
                            ErrorCode.DocumentAlreadyExists,
                            docCheck.Message ?? ErrorMessages.DocumentAlreadyExists);
                    }
                }

                var emailCheck = await _uniquenessValidation.CheckEmailAsync(normalizedEmail, cancellationToken: cancellationToken);
                if (!emailCheck.IsAvailable)
                {
                    return ApiResponse<ProfessionalResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        emailCheck.Message ?? ErrorMessages.EmailAlreadyRegistered);
                }

                if (normalizedLicense is not null)
                {
                    var licenseCheck = await _uniquenessValidation.CheckLicenseNumberAsync(normalizedLicense, null, cancellationToken);
                    if (!licenseCheck.IsAvailable)
                    {
                        return ApiResponse<ProfessionalResponse>.Conflict(
                            ErrorCode.LicenseNumberAlreadyExists,
                            licenseCheck.Message ?? "Esta matrícula ya se encuentra registrada.");
                    }
                }

                var userId = Guid.NewGuid();
                var tempPassword = PasswordGenerator.GenerateTemporary();

                var user = new User
                {
                    Id = userId,
                    Name = command.FirstName.Trim(),
                    Surname = command.LastName.Trim(),
                    Email = normalizedEmail,
                    UserName = normalizedEmail,
                    EmailConfirmed = false,
                    IsActive = false,
                    CreatedAt = _dateTime.UtcNow
                };

                var professional = new Professional
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    FirstName = command.FirstName.Trim(),
                    LastName = command.LastName.Trim(),
                    DocumentNumber = normalizedDoc,
                    Phone = command.Phone?.Trim(),
                    Specialty = command.Specialty,
                    SpecialtyId = command.SpecialtyId,
                    LicenseNumber = normalizedLicense,
                    BirthDate = command.BirthDate,
                    Email = normalizedEmail,
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

                string? userCreateErrors = null;
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    var createUserResult = await _identityService.CreateUserAsync(user, tempPassword);
                    if (!createUserResult.Succeeded)
                    {
                        userCreateErrors = string.Join(", ", createUserResult.Errors);
                        throw new InvalidOperationException($"Error al crear usuario: {userCreateErrors}");
                    }

                    await _identityService.AddToRoleAsync(user, IdentityRoles.Professional.ToString());
                    professional.UserId = user.Id;
                    await _repository.CreateAsync(professional, ct);
                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                _logger.LogInformation("Profesional registrado (pendiente): {ProfessionalId}, UserId: {UserId}, Email: {Email}", 
                    professional.Id, userId, normalizedEmail);

                await _backgroundJobs.CreateAsync(
                    JobTypes.Email,
                    JsonSerializer.Serialize(new EmailPayload
                    {
                        To           = normalizedEmail,
                        Subject      = "Tu registro en InclusiON está pendiente de validación",
                        TemplateName = "ProfessionalPendingRegistration",
                        Replacements = new Dictionary<string, string?>
                        {
                            { "FirstName", command.FirstName.Trim() },
                            { "LastName", command.LastName.Trim() },
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
