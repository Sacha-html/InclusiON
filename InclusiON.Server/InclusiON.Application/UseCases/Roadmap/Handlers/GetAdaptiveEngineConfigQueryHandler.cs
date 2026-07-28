using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers;

public class GetAdaptiveEngineConfigQueryHandler(IAdaptiveEngineRepository adaptiveRepo)
    : IQueryHandler<GetAdaptiveEngineConfigQuery, ApiResponse<AdaptiveEngineConfigResponse?>>
{
    public async Task<ApiResponse<AdaptiveEngineConfigResponse?>> HandleAsync(
        GetAdaptiveEngineConfigQuery query, CancellationToken cancellationToken = default)
    {
        var config = await adaptiveRepo.GetConfigAsync(query.PersonRoadmapActivityId, cancellationToken);
        if (config is null)
            return ApiResponse<AdaptiveEngineConfigResponse?>.SuccessResult(null);

        return ApiResponse<AdaptiveEngineConfigResponse?>.SuccessResult(AdaptiveEngineConfigResponse.From(config));
    }
}
