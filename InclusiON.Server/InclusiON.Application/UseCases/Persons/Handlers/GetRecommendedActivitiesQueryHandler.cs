using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class GetRecommendedActivitiesQueryHandler
        : IQueryHandler<GetRecommendedActivitiesQuery, ApiResponse<List<ActivityListItemResponse>>>
    {
        private readonly IEmbeddingRepository _embeddingRepository;
        private readonly IActivitiesRepository _activitiesRepository;
        private readonly IEncryptionService _encryption;
        private readonly ILogger<GetRecommendedActivitiesQueryHandler> _logger;

        public GetRecommendedActivitiesQueryHandler(
            IEmbeddingRepository embeddingRepository,
            IActivitiesRepository activitiesRepository,
            IEncryptionService encryption,
            ILogger<GetRecommendedActivitiesQueryHandler> logger)
        {
            _embeddingRepository   = embeddingRepository;
            _activitiesRepository  = activitiesRepository;
            _encryption            = encryption;
            _logger                 = logger;
        }

        public async Task<ApiResponse<List<ActivityListItemResponse>>> HandleAsync(
            GetRecommendedActivitiesQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var activityIds = await _embeddingRepository.SearchActivitiesForPersonAsync(
                    query.PersonId,
                    query.ProfessionalId,
                    query.Limit,
                    cancellationToken);

                if (activityIds.Count == 0)
                    return ApiResponse<List<ActivityListItemResponse>>.SuccessResult([]);

                var activities = await _activitiesRepository.GetByIdsAsync(activityIds, cancellationToken);

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
                _logger.LogError(ex, "Error al buscar actividades recomendadas para persona {PersonId}", query.PersonId);
                return ApiResponse<List<ActivityListItemResponse>>.ErrorResult(
                    ErrorCode.InternalError, "Error al buscar actividades recomendadas.");
            }
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}