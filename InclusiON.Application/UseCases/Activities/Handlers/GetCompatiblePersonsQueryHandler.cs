using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class GetCompatiblePersonsQueryHandler
        : IQueryHandler<GetCompatiblePersonsQuery, ApiResponse<List<PersonListItemResponse>>>
    {
        private readonly IEmbeddingRepository _embeddingRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IEncryptionService _encryption;
        private readonly ILogger<GetCompatiblePersonsQueryHandler> _logger;

        public GetCompatiblePersonsQueryHandler(
            IEmbeddingRepository embeddingRepository,
            IPersonsRepository personsRepository,
            IEncryptionService encryption,
            ILogger<GetCompatiblePersonsQueryHandler> logger)
        {
            _embeddingRepository  = embeddingRepository;
            _personsRepository    = personsRepository;
            _encryption           = encryption;
            _logger                = logger;
        }

        public async Task<ApiResponse<List<PersonListItemResponse>>> HandleAsync(
            GetCompatiblePersonsQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var personIds = await _embeddingRepository.SearchPersonsForActivityAsync(
                    query.ActivityId,
                    query.ProfessionalId,
                    query.Limit,
                    cancellationToken);

                if (personIds.Count == 0)
                    return ApiResponse<List<PersonListItemResponse>>.SuccessResult([]);

                var persons = await _personsRepository.GetByIdsAsync(personIds, cancellationToken);

                var result = persons.Select(p =>
                {
                    var item = PersonListItemResponse.MapToResponse(p);
                    item.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(p.Id.ToString()));
                    return item;
                }).ToList();

                return ApiResponse<List<PersonListItemResponse>>.SuccessResult(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al buscar personas compatibles para actividad {ActivityId}", query.ActivityId);
                return ApiResponse<List<PersonListItemResponse>>.ErrorResult(
                    ErrorCode.InternalError, "Error al buscar personas compatibles.");
            }
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}