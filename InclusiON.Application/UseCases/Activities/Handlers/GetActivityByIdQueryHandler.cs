using InclusiON.Application.Interfaces.Common;
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

        public GetActivityByIdQueryHandler(IActivitiesRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<ActivityResponse>> HandleAsync(
            GetActivityByIdQuery query, CancellationToken cancellationToken)
        {
            var activity = await _repository.GetByIdAsync(query.ActivityId, cancellationToken);

            if (activity is null)
                return ApiResponse<ActivityResponse>.NotFound("Actividad");

            // Profesional solo puede ver sus propias actividades o las estándar
            if (!activity.IsStandardActivity && activity.ProfessionalId != query.ProfessionalId)
                return ApiResponse<ActivityResponse>.Forbidden();

            return ApiResponse<ActivityResponse>.SuccessResult(ActivityResponse.From(activity));
        }
    }
}
