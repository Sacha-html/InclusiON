using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class GetActivityByIdQueryHandler
        : IQueryHandler<GetActivityByIdQuery, ApiResponse<ActivityResponse>>
    {
        private readonly IActivitiesRepository _repository;
        private readonly IEncryptionService _encryption;

        public GetActivityByIdQueryHandler(IActivitiesRepository repository, IEncryptionService encryption)
        {
            _repository = repository;
            _encryption = encryption;
        }

        public async Task<ApiResponse<ActivityResponse>> HandleAsync(
            GetActivityByIdQuery query, CancellationToken cancellationToken)
        {
            var activity = await _repository.GetByIdAsync(query.ActivityId, cancellationToken);

            if (activity is null)
                return ApiResponse<ActivityResponse>.NotFound("Actividad");

            var dto = ActivityResponse.From(activity);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(activity.Id.ToString()));
            return ApiResponse<ActivityResponse>.SuccessResult(dto);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
