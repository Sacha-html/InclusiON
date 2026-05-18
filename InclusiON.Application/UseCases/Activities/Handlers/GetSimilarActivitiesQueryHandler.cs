using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class GetSimilarActivitiesQueryHandler
        : IQueryHandler<GetSimilarActivitiesQuery, ApiResponse<List<ActivityListItemResponse>>>
    {
        private readonly IEmbeddingRepository _embeddingRepository;
        private readonly IActivitiesRepository _activitiesRepository;
        private readonly IEncryptionService _encryption;
        private readonly ILogger<GetSimilarActivitiesQueryHandler> _logger;

        public GetSimilarActivitiesQueryHandler(
            IEmbeddingRepository embeddingRepository,
            IActivitiesRepository activitiesRepository,
            IEncryptionService encryption,
            ILogger<GetSimilarActivitiesQueryHandler> logger)
        {
            _embeddingRepository     = embeddingRepository;
            _activitiesRepository   = activitiesRepository;
            _encryption             = encryption;
            _logger                 = logger;
        }

        public async Task<ApiResponse<List<ActivityListItemResponse>>> HandleAsync(
            GetSimilarActivitiesQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Obtener embedding de la actividad
                var activityEmbedding = await _embeddingRepository.GetByActivityIdAsync(
                    query.ActivityId, cancellationToken);

                if (activityEmbedding is null)
                    return ApiResponse<List<ActivityListItemResponse>>.SuccessResult([]);

                // 2. Buscar actividades similares (excluyendo la misma)
                var similarIds = await _embeddingRepository.SearchAsync(
                    activityEmbedding,
                    query.ProfessionalId,
                    query.Limit + 1, // +1 por si devuelve la misma
                    [query.ActivityId], // excludeIds
                    cancellationToken);

                // Remover la actividad original si está en la lista
                similarIds = similarIds.Where(id => id != query.ActivityId).Take(query.Limit).ToList();

                if (similarIds.Count == 0)
                    return ApiResponse<List<ActivityListItemResponse>>.SuccessResult([]);

                // 3. Cargar actividades
                var activities = await _activitiesRepository.GetByIdsAsync(similarIds, cancellationToken);

                var result = activities.Select(a =>
                {
                    var item = ActivityListItemResponse.From(a);
                    item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(a.Id.ToString()));
                    return item;
                }).ToList();

                return ApiResponse<List<ActivityListItemResponse>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar actividades similares para {ActivityId}", query.ActivityId);
                return ApiResponse<List<ActivityListItemResponse>>.ErrorResult(
                    ErrorCode.InternalError, "Error al buscar actividades similares.");
            }
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}