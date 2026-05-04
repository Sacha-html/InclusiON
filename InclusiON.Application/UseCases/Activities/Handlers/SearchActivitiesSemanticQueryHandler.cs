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
    public class SearchActivitiesSemanticQueryHandler
        : IQueryHandler<SearchActivitiesSemanticQuery, ApiResponse<List<ActivityListItemResponse>>>
    {
        private readonly IEmbeddingService _embeddingService;
        private readonly IEmbeddingRepository _embeddingRepository;
        private readonly IActivitiesRepository _activitiesRepository;
        private readonly ILogger<SearchActivitiesSemanticQueryHandler> _logger;

        public SearchActivitiesSemanticQueryHandler(
            IEmbeddingService embeddingService,
            IEmbeddingRepository embeddingRepository,
            IActivitiesRepository activitiesRepository,
            ILogger<SearchActivitiesSemanticQueryHandler> logger)
        {
            _embeddingService        = embeddingService;
            _embeddingRepository     = embeddingRepository;
            _activitiesRepository    = activitiesRepository;
            _logger                  = logger;
        }

        public async Task<ApiResponse<List<ActivityListItemResponse>>> HandleAsync(
            SearchActivitiesSemanticQuery query, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Generar embedding del texto de búsqueda
                var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(
                    query.Text, cancellationToken);

                // 2. Buscar por similitud coseno → lista de IDs ordenada
                var ids = await _embeddingRepository.SearchAsync(
                    queryEmbedding,
                    query.ProfessionalId,
                    query.Limit,
                    cancellationToken);

                if (ids.Count == 0)
                    return ApiResponse<List<ActivityListItemResponse>>.SuccessResult([]);

                // 3. Cargar entidades (con includes) preservando orden de similitud
                var activities = await _activitiesRepository.GetByIdsAsync(ids, cancellationToken);

                var result = activities
                    .Select(ActivityListItemResponse.From)
                    .ToList();

                return ApiResponse<List<ActivityListItemResponse>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda semántica para profesional {ProfessionalId}", query.ProfessionalId);
                return ApiResponse<List<ActivityListItemResponse>>.ErrorResult(
                    ErrorCode.InternalError, "Error al ejecutar la búsqueda semántica.");
            }
        }
    }
}
