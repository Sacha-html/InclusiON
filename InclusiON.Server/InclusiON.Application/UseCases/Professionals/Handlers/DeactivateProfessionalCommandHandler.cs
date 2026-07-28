using Microsoft.Extensions.Logging;
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
    public class DeactivateProfessionalCommandHandler : ICommandHandler<DeactivateProfessionalCommand, ApiResponse<ProfessionalResponse>>
    {
        private readonly IProfessionalsRepository _repository;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IHttpContextService _httpContextService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeactivateProfessionalCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public DeactivateProfessionalCommandHandler(
            IProfessionalsRepository repository,
            IRefreshTokensRepository refreshTokensRepository,
            IHttpContextService httpContextService,
            IUnitOfWork unitOfWork,
            ILogger<DeactivateProfessionalCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _refreshTokensRepository = refreshTokensRepository;
            _httpContextService = httpContextService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ProfessionalResponse>> HandleAsync(DeactivateProfessionalCommand command, CancellationToken cancellationToken)
        {
            var professional = await _repository.GetByIdAsync(command.ProfessionalId, cancellationToken);

            if (professional == null)
            {
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound);
            }

            if (professional.Status == ProfessionalStatusEnum.Terminated)
            {
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.BusinessRuleViolation,
                    "El profesional ya se encuentra dado de baja");
            }

            var dependentPersonsCount = await _repository.GetDependentAssistedLoginPersonsCountAsync(professional.UserId, cancellationToken);
            if (dependentPersonsCount > 0)
            {
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.InvalidOperation,
                    $"No se puede desactivar al profesional porque es el supervisor exclusivo de inicio de sesión asistido para {dependentPersonsCount} alumno(s). Reasigne la supervisión de estos alumnos antes de proceder.");
            }

            var adminUserId = _httpContextService.GetCurrentUserId();
            if (!adminUserId.HasValue)
            {
                return ApiResponse<ProfessionalResponse>.Unauthorized();
            }

            var oldStatus = professional.Status;

            professional.User.IsActive = false;
            professional.Status = ProfessionalStatusEnum.Terminated;

            foreach (var pi in professional.ProfessionalInstitutions)
            {
                pi.IsActive = false;
            }

            await _repository.AddStatusHistoryAsync(new ProfessionalStatusHistory
            {
                ProfessionalId = professional.Id,
                OldStatus = oldStatus,
                NewStatus = ProfessionalStatusEnum.Terminated,
                Observation = command.Observation,
                ChangedByUserId = adminUserId,
                CreatedAt = _dateTime.UtcNow,
                CreatedBy = adminUserId.Value
            }, cancellationToken);

            await _refreshTokensRepository.RevokeAllUserTokensAsync(
                professional.UserId,
                Constants.RevokeReasons.ProfessionalDeactivated,
                cancellationToken);

            await _repository.UpdateAsync(professional, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profesional dado de baja: {ProfessionalId}, Usuario: {UserId}", command.ProfessionalId, professional.UserId);

            var response = ProfessionalResponse.MapToResponse(professional);
            return ApiResponse<ProfessionalResponse>.SuccessResult(response, SuccessMessages.ProfessionalDeactivated);
        }
    }
}
