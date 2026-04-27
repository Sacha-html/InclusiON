using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Application.UseCases.Professionals.Handlers
{
    public class SuspendInactiveProfessionalsCommandHandler : ICommandHandler<SuspendInactiveProfessionalsCommand, ApiResponse<SuspendResult>>
    {
        private readonly IProfessionalsRepository _repository;
        private readonly IHttpContextService _httpContextService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SuspendInactiveProfessionalsCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public SuspendInactiveProfessionalsCommandHandler(
            IProfessionalsRepository repository,
            IHttpContextService httpContextService,
            IUnitOfWork unitOfWork,
            ILogger<SuspendInactiveProfessionalsCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _httpContextService = httpContextService;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<SuspendResult>> HandleAsync(SuspendInactiveProfessionalsCommand command, CancellationToken cancellationToken)
        {
            var inactiveProfessionals = await _repository.GetInactiveProfessionalsAsync(command.InactiveDays, cancellationToken);

            var adminUserId = _httpContextService.GetCurrentUserId() ?? Guid.Empty;
            var suspendedCount = 0;

            foreach (var prof in inactiveProfessionals)
            {
                prof.Status = ProfessionalStatusEnum.Suspended;
                foreach (var pi in prof.ProfessionalInstitutions)
                {
                    pi.IsActive = false;
                }

                await _repository.AddStatusHistoryAsync(new ProfessionalStatusHistory
                {
                    ProfessionalId = prof.Id,
                    OldStatus = ProfessionalStatusEnum.Approved,
                    NewStatus = ProfessionalStatusEnum.Suspended,
                    Observation = $"Suspendido por inactividad ({command.InactiveDays} días sin acceder)",
                    ChangedByUserId = adminUserId,
                    CreatedAt = _dateTime.UtcNow,
                    CreatedBy = adminUserId
                }, cancellationToken);

                suspendedCount++;
            }

            if (suspendedCount > 0)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("{Count} profesionales suspendidos por inactividad ({Days} días)", suspendedCount, command.InactiveDays);

            return ApiResponse<SuspendResult>.SuccessResult(
                new SuspendResult { SuspendedCount = suspendedCount },
                $"{suspendedCount} profesionales suspendidos por inactividad");
        }
    }
}
