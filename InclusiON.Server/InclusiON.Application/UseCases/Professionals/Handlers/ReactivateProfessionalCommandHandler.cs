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
    public class ReactivateProfessionalCommandHandler : ICommandHandler<ReactivateProfessionalCommand, ApiResponse<ProfessionalResponse>>
    {
        private readonly IProfessionalsRepository _repository;
        private readonly IHttpContextService _httpContextService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReactivateProfessionalCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public ReactivateProfessionalCommandHandler(
            IProfessionalsRepository repository,
            IHttpContextService httpContextService,
            IUnitOfWork unitOfWork,
            ILogger<ReactivateProfessionalCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _httpContextService = httpContextService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ProfessionalResponse>> HandleAsync(ReactivateProfessionalCommand command, CancellationToken cancellationToken)
        {
            var professional = await _repository.GetByIdAsync(command.ProfessionalId, cancellationToken);

            if (professional == null)
            {
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.ProfessionalNotFound,
                    ErrorMessages.ProfessionalNotFound);
            }

            if (professional.Status == ProfessionalStatusEnum.Approved && professional.User.IsActive)
            {
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.BusinessRuleViolation,
                    "El profesional ya se encuentra activo");
            }

            var adminUserId = _httpContextService.GetCurrentUserId();
            if (!adminUserId.HasValue)
            {
                return ApiResponse<ProfessionalResponse>.Unauthorized();
            }

            var oldStatus = professional.Status;
            professional.User.IsActive = true;
            professional.Status = ProfessionalStatusEnum.Approved;

            foreach (var pi in professional.ProfessionalInstitutions)
            {
                pi.IsActive = true;
            }

            await _repository.AddStatusHistoryAsync(new ProfessionalStatusHistory
            {
                ProfessionalId = professional.Id,
                OldStatus = oldStatus,
                NewStatus = ProfessionalStatusEnum.Approved,
                Observation = command.Observation,
                ChangedByUserId = adminUserId,
                CreatedAt = _dateTime.UtcNow,
                CreatedBy = adminUserId.Value
            }, cancellationToken);

            await _repository.UpdateAsync(professional, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Profesional reactivado: {ProfessionalId}, Usuario: {UserId}", command.ProfessionalId, professional.UserId);

            var response = ProfessionalResponse.MapToResponse(professional);
            return ApiResponse<ProfessionalResponse>.SuccessResult(response, "Profesional reactivado exitosamente");
        }
    }
}
