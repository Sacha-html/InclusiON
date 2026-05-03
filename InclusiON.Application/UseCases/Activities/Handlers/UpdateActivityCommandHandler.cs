using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class UpdateActivityCommandHandler
        : ICommandHandler<UpdateActivityCommand, ApiResponse<ActivityResponse>>
    {
        private readonly IActivitiesRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEmbeddingService _embeddingService;
        private readonly IEmbeddingRepository _embeddingRepository;
        private readonly ILogger<UpdateActivityCommandHandler> _logger;

        public UpdateActivityCommandHandler(
            IActivitiesRepository repository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEmbeddingService embeddingService,
            IEmbeddingRepository embeddingRepository,
            ILogger<UpdateActivityCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
            _embeddingService = embeddingService;
            _embeddingRepository = embeddingRepository;
            _logger = logger;
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

                _ = GenerateAndStoreEmbeddingAsync(activity.Id, command.Title, command.Description, command.Instructions);

                return ApiResponse<ActivityResponse>.SuccessResult(
                    ActivityResponse.From(updated!),
                    "Actividad actualizada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar actividad {ActivityId}", command.ActivityId);
                return ApiResponse<ActivityResponse>.ErrorResult(ErrorCode.InternalError, "Error interno al actualizar la actividad.");
            }
        }

        private async Task GenerateAndStoreEmbeddingAsync(int activityId, string title, string? description, string? instructions)
        {
            try
            {
                var parts = new[] { title, description, instructions }
                    .Where(p => !string.IsNullOrWhiteSpace(p));
                var text = string.Join(". ", parts);

                var embedding = await _embeddingService.GenerateEmbeddingAsync(text);
                await _embeddingRepository.StoreAsync(activityId, embedding);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo regenerar embedding para actividad {ActivityId}", activityId);
            }
        }
    }
}
