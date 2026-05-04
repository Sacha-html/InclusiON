using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;
using System.Text.Json;
using Activity = InclusiON.Domain.Models.Activity;
using ActivityResponse = InclusiON.DTOs.Responses.Activities.ActivityResponse;
using ActivityContent = InclusiON.Domain.Models.ActivityContent;
using ActivityEmbedding = InclusiON.Domain.Models.ActivityEmbedding;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class CreateActivityCommandHandler
        : ICommandHandler<CreateActivityCommand, ApiResponse<ActivityResponse>>
    {
        private readonly IActivitiesRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEmbeddingService _embeddingService;
        private readonly IEmbeddingRepository _embeddingRepository;
        private readonly ILogger<CreateActivityCommandHandler> _logger;

        public CreateActivityCommandHandler(
            IActivitiesRepository repository,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEmbeddingService embeddingService,
            IEmbeddingRepository embeddingRepository,
            ILogger<CreateActivityCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _dateTime = dateTime;
            _embeddingService = embeddingService;
            _embeddingRepository = embeddingRepository;
            _logger = logger;
        }

        public async Task<ApiResponse<ActivityResponse>> HandleAsync(
            CreateActivityCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Validar que ContentJson sea JSON parseable y no un objeto vacío
                if (!TryParseContentJson(command.ContentJson, out var jsonDoc))
                    return ApiResponse<ActivityResponse>.ErrorResult(
                        ErrorCode.ValidationFailed,
                        "El contenido de la actividad no es un JSON válido.");

                if (jsonDoc?.RootElement.ValueKind == JsonValueKind.Object
                    && jsonDoc.RootElement.EnumerateObject().MoveNext() == false)
                    return ApiResponse<ActivityResponse>.ErrorResult(
                        ErrorCode.ValidationFailed,
                        "El contenido de la actividad está vacío. Completá el editor antes de guardar.");

                jsonDoc?.Dispose();

                var now = _dateTime.UtcNow;

                var activity = new Activity
                {
                    ProfessionalId           = command.ProfessionalId,
                    Title                    = command.Title,
                    Description              = command.Description,
                    Instructions             = command.Instructions,
                    CategoryId               = command.CategoryId,
                    SkillAreaId              = command.SkillAreaId,
                    ComplexityLevel          = command.ComplexityLevel,
                    EstimatedDurationMinutes = command.EstimatedDurationMinutes,
                    RequiresSupervision      = command.RequiresSupervision,
                    HasVisualSupport         = command.HasVisualSupport,
                    HasAudioSupport          = command.HasAudioSupport,
                    UsesEasyReading          = command.UsesEasyReading,
                    UsesPictograms           = command.UsesPictograms,
                    ResourcesUrl             = command.ResourcesUrl,
                    IsStandardActivity       = false,
                    IsActive                 = true,
                    CreatedAt                = now,
                };

                activity.Content = new ActivityContent
                {
                    TemplateTypeId = command.TemplateTypeId,
                    ContentJson    = command.ContentJson,
                };

                activity.Embedding = new ActivityEmbedding
                {
                    Model      = "paraphrase-multilingual-MiniLM-L12-v2",
                    Dimensions = 384,
                    CreatedAt  = now,
                };

                await _repository.CreateAsync(activity, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Reload con includes para devolver respuesta completa
                var created = await _repository.GetByIdAsync(activity.Id, cancellationToken);

                _ = GenerateAndStoreEmbeddingAsync(activity.Id, command.Title, command.Description, command.Instructions);

                return ApiResponse<ActivityResponse>.SuccessResult(
                    ActivityResponse.From(created!),
                    "Actividad creada exitosamente.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear actividad para profesional {ProfessionalId}", command.ProfessionalId);
                return ApiResponse<ActivityResponse>.ErrorResult(ErrorCode.InternalError, "Error interno al crear la actividad.");
            }
        }

        private static bool TryParseContentJson(string json, out JsonDocument? doc)
        {
            doc = null;
            try { doc = JsonDocument.Parse(json); return true; }
            catch { return false; }
        }

        private async Task GenerateAndStoreEmbeddingAsync(int activityId, string title, string? description, string? instructions)
        {
            try
            {
                var parts = new[] { title, description, instructions }
                    .Where(p => !string.IsNullOrWhiteSpace(p));
                var text = string.Join(". ", parts);

                var embedding = await _embeddingService.GenerateEmbeddingAsync(text);
                if (embedding.Length == 0) return;
                await _embeddingRepository.StoreAsync(activityId, embedding);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo generar embedding para actividad {ActivityId}", activityId);
            }
        }
    }
}
