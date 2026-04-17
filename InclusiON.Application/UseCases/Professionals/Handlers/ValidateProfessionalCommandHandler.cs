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
    public class ValidateProfessionalCommandHandler : ICommandHandler<ValidateProfessionalCommand, ApiResponse<ProfessionalResponse>>
    {
        private readonly IProfessionalsRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IEmailService _emailService;
        private readonly IAdminInstitutionRepository _adminInstitutionRepository;
        private readonly IHttpContextService _httpContextService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ValidateProfessionalCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public ValidateProfessionalCommandHandler(
            IProfessionalsRepository repository,
            IIdentityService identityService,
            IEmailService emailService,
            IAdminInstitutionRepository adminInstitutionRepository,
            IHttpContextService httpContextService,
            IUnitOfWork unitOfWork,
            ILogger<ValidateProfessionalCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _identityService = identityService;
            _emailService = emailService;
            _adminInstitutionRepository = adminInstitutionRepository;
            _httpContextService = httpContextService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ProfessionalResponse>> HandleAsync(ValidateProfessionalCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var professional = await _repository.GetByIdAsync(command.ProfessionalId, cancellationToken);
                if (professional == null)
                {
                    return ApiResponse<ProfessionalResponse>.NotFound("Profesional");
                }

                if (professional.Status != ProfessionalStatusEnum.Pending)
                {
                    return ApiResponse<ProfessionalResponse>.ErrorResult(
                        ErrorCode.BusinessRuleViolation,
                        "El profesional ya ha sido validado anteriormente");
                }

                var adminUserId = _httpContextService.GetCurrentUserId();
                if (!adminUserId.HasValue)
                {
                    return ApiResponse<ProfessionalResponse>.Unauthorized();
                }

                var adminInstitutionIds = await _adminInstitutionRepository.GetActiveInstitutionIdsByAdminAsync(adminUserId.Value, cancellationToken);
                var isGlobalAdmin = adminInstitutionIds.Count == 0;

                var professionalInstitutionIds = await _repository.GetInstitutionIdsAsync(professional.Id, cancellationToken);

                if (!isGlobalAdmin && professionalInstitutionIds.Count > 0)
                {
                    var hasAccess = professionalInstitutionIds.Any(id => adminInstitutionIds.Contains(id));
                    if (!hasAccess)
                    {
                        return ApiResponse<ProfessionalResponse>.Forbidden(
                            "No tienes permiso para validar profesionales de otras instituciones");
                    }
                }

                if (!isGlobalAdmin && professionalInstitutionIds.Count == 0)
                {
                    return ApiResponse<ProfessionalResponse>.Forbidden(
                        "Solo un administrador global puede validar profesionales sin institución");
                }

                var oldStatus = professional.Status;
                var newStatus = command.IsApproved ? ProfessionalStatusEnum.Approved : ProfessionalStatusEnum.Rejected;

                if (command.IsApproved)
                {
                    var password = PasswordGenerator.GenerateTemporary();

                    User? userToActivate = null;

                    await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                    {
                        userToActivate = await _identityService.FindByIdAsync(professional.UserId);
                        if (userToActivate == null)
                        {
                            throw new InvalidOperationException("El profesional no tiene un usuario asociado. Debe registrarse primero.");
                        }

                        userToActivate.IsActive = true;
                        userToActivate.EmailConfirmed = true;
                        userToActivate.MustChangePassword = true;

                        await _identityService.UpdateUserAsync(userToActivate);

                        var resetResult = await _identityService.ResetPasswordAsync(userToActivate, password);
                        if (!resetResult.Succeeded)
                        {
                            throw new InvalidOperationException(string.Format(ErrorMessages.UserCreationError, string.Join(", ", resetResult.Errors)));
                        }

                        professional.Status = ProfessionalStatusEnum.Approved;
                        professional.ValidatedAt = _dateTime.UtcNow;
                        professional.ValidatedByUserId = adminUserId;

                        await _repository.UpdateAsync(professional, ct);

                        var history = new ProfessionalStatusHistory
                        {
                            ProfessionalId = professional.Id,
                            OldStatus = oldStatus,
                            NewStatus = newStatus,
                            Observation = command.Observation,
                            ChangedByUserId = adminUserId,
                            CreatedAt = _dateTime.UtcNow,
                            CreatedBy = adminUserId.Value
                        };

                        await _unitOfWork.SaveChangesAsync(ct);
                    }, cancellationToken);

                    _logger.LogInformation("Profesional aprobado: {ProfessionalId}, Usuario: {UserId}",
                        professional.Id, professional.UserId);

                    // Enviar email de aprobación sin bloquear la respuesta
                    var email = userToActivate.Email ?? "";
                    var firstName = professional.FirstName;
                    var tempPassword = password;
                    if (!string.IsNullOrEmpty(email))
                    {
                        // TODO: Refactorizar usando Microsoft.Extensions.AI / Semantic Kernel Agent Framework
                        // para orquestar notificaciones de forma inteligente (reintentos, canales múltiples, prioridad).
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await _emailService.SendTemplatedEmailAsync(
                                    email,
                                    "Tu cuenta en InclusiON ha sido validada",
                                    "ProfessionalApproved",
                                    new Dictionary<string, string?>
                                    {
                                        { "UserName", firstName },
                                        { "Email", email },
                                        { "TemporaryPassword", tempPassword },
                                        { "LoginUrl", "https://inclusion.app/login" },
                                        { "Year", _dateTime.UtcNow.Year.ToString() }
                                    });
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "No se pudo enviar email de aprobación a {Email}", email);
                            }
                        });
                    }
                }
                else
                {
                    professional.Status = ProfessionalStatusEnum.Rejected;
                    professional.ValidatedAt = _dateTime.UtcNow;
                    professional.ValidatedByUserId = adminUserId;

                    // Desactivar relaciones con instituciones
                    foreach (var pi in professional.ProfessionalInstitutions)
                    {
                        pi.IsActive = false;
                    }

                    var history = new ProfessionalStatusHistory
                    {
                        ProfessionalId = professional.Id,
                        OldStatus = oldStatus,
                        NewStatus = newStatus,
                        Observation = command.Observation,
                        ChangedByUserId = adminUserId,
                        CreatedAt = _dateTime.UtcNow,
                        CreatedBy = adminUserId.Value
                    };

                    await _repository.UpdateAsync(professional, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation("Profesional rechazado: {ProfessionalId}", professional.Id);

                    // TODO: Refactorizar usando Microsoft.Extensions.AI / Semantic Kernel Agent Framework
                    // para orquestar notificaciones de forma inteligente (reintentos, canales múltiples, prioridad).
                    // Enviar email de rechazo sin bloquear la respuesta
                    var rejectEmail = professional.Email ?? $"{professional.FirstName.ToLower()}.{professional.LastName.ToLower()}@inclusion.app";
                    var rejectFirstName = professional.FirstName;
                    var rejectObservation = command.Observation ?? "Sin observación";
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendTemplatedEmailAsync(
                                rejectEmail,
                                "Tu registro en InclusiON ha sido rechazado",
                                "ProfessionalRejected",
                                new Dictionary<string, string?>
                                {
                                    { "UserName", rejectFirstName },
                                    { "Observation", rejectObservation },
                                    { "Year", _dateTime.UtcNow.Year.ToString() }
                                });
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "No se pudo enviar email de rechazo a {Email}", rejectEmail);
                        }
                    });
                }

                var response = ProfessionalResponse.MapToResponse(professional);
                var message = command.IsApproved
                    ? SuccessMessages.ProfessionalValidated
                    : SuccessMessages.ProfessionalRejected;

                return ApiResponse<ProfessionalResponse>.SuccessResult(response, message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al aprobar profesional");
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al validar profesional: {ProfessionalId}", command.ProfessionalId);
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    ErrorMessages.InternalErrorRegister);
            }
        }
    }
}
