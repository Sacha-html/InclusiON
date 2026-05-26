using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;
using ActivityResponse = InclusiON.DTOs.Responses.Activities.ActivityResponse;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class PatchActivityStatusCommandHandler
        : ICommandHandler<PatchActivityStatusCommand, ApiResponse<ActivityResponse>>
    {
        private readonly IActivitiesRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;

        public PatchActivityStatusCommandHandler(
            IActivitiesRepository repository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEncryptionService encryption)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
            _encryption = encryption;
        }

        public async Task<ApiResponse<ActivityResponse>> HandleAsync(
            PatchActivityStatusCommand command, CancellationToken cancellationToken)
        {
            var activity = await _repository.GetByIdAsync(command.ActivityId, cancellationToken);

            if (activity is null)
                return ApiResponse<ActivityResponse>.NotFound("Actividad");

            if (activity.IsStandardActivity || activity.ProfessionalId != command.ProfessionalId)
                return ApiResponse<ActivityResponse>.Forbidden();

            if (activity.IsActive == command.IsActive)
                return ApiResponse<ActivityResponse>.Conflict(
                    ErrorCode.BusinessRuleViolation,
                    $"La actividad ya está {(command.IsActive ? "activa" : "inactiva")}.");

            if (!command.IsActive)
            {
                var hasActive = await _repository.HasActiveAssignmentsAsync(command.ActivityId, cancellationToken);
                if (hasActive)
                    return ApiResponse<ActivityResponse>.Conflict(
                        ErrorCode.BusinessRuleViolation,
                        "No se puede dar de baja una actividad con asignaciones activas.");
            }

            activity.IsActive  = command.IsActive;
            activity.UpdatedAt = _dateTime.UtcNow;

            await _repository.UpdateAsync(activity, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await _repository.GetByIdAsync(activity.Id, cancellationToken);

            var dto = ActivityResponse.From(updated!);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(updated!.Id.ToString()));
            return ApiResponse<ActivityResponse>.SuccessResult(
                dto,
                command.IsActive ? "Actividad reactivada." : "Actividad dada de baja.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
