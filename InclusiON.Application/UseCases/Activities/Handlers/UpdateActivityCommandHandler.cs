using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;
using System.Text.Json;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class UpdateActivityCommandHandler
        : ICommandHandler<UpdateActivityCommand, ApiResponse<ActivityResponse>>
    {
        private readonly IActivitiesRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;
        private readonly IBackgroundJobRepository _backgroundJobRepository;
        private readonly ILogger<UpdateActivityCommandHandler> _logger;

        public UpdateActivityCommandHandler(
            IActivitiesRepository repository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEncryptionService encryption,
            IBackgroundJobRepository backgroundJobRepository,
            ILogger<UpdateActivityCommandHandler> logger)
        {
            _repository       = repository;
            _unitOfWork       = unitOfWork;
            _dateTime         = dateTime;
            _encryption       = encryption;
            _backgroundJobRepository = backgroundJobRepository;
            _logger           = logger;
        }

        public async Task<ApiResponse<ActivityResponse>> HandleAsync(
            UpdateActivityCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var activity = await _repository.GetByIdAsync(command.ActivityId, cancellationToken);

                if (activity is null)
                    return ApiResponse<ActivityResponse>.NotFound("Actividad");

                if (activity.IsStandardActivity || activity.ProfessionalId != command.ProfessionalId)
                    return ApiResponse<ActivityResponse>.Forbidden();

                var now = _dateTime.UtcNow;

                activity.Title                    = command.Title;
                activity.Description              = command.Description;
                activity.Instructions             = command.Instructions;
                activity.CategoryId               = command.CategoryId;
                activity.SkillAreaId              = command.SkillAreaId;
                activity.ComplexityLevel          = command.ComplexityLevel;
                activity.EstimatedDurationMinutes = command.EstimatedDurationMinutes;
                activity.RequiresSupervision      = command.RequiresSupervision;
                activity.HasVisualSupport         = command.HasVisualSupport;
                activity.HasAudioSupport          = command.HasAudioSupport;
                activity.UsesEasyReading          = command.UsesEasyReading;
                activity.UsesPictograms           = command.UsesPictograms;
                activity.ResourcesUrl             = command.ResourcesUrl;
                activity.UpdatedAt                = now;

                if (activity.Content is not null)
                {
                    activity.Content.ContentJson = command.ContentJson;
                }

                await _repository.UpdateAsync(activity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                var updated = await _repository.GetByIdAsync(activity.Id, cancellationToken);

                var payload = JsonSerializer.Serialize(new
                {
                    entity_type = "activity",
                    entity_id = activity.Id.ToString(),
                    title = command.Title,
                    description = command.Description,
                    instructions = command.Instructions,
                    content_json = command.ContentJson,
                });

                await _backgroundJobRepository.CreateAsync(
                    JobTypes.Embedding, payload, maxRetries: 3, cancellationToken: cancellationToken);

                var dto = ActivityResponse.From(updated!);
                dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(updated!.Id.ToString()));
                return ApiResponse<ActivityResponse>.SuccessResult(dto, "Actividad actualizada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar actividad {ActivityId}", command.ActivityId);
                return ApiResponse<ActivityResponse>.ErrorResult(ErrorCode.InternalError, "Error interno al actualizar la actividad.");
            }
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
