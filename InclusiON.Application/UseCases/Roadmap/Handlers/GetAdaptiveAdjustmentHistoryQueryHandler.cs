using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers;

public class GetAdaptiveAdjustmentHistoryQueryHandler(
    IAdaptiveEngineRepository adaptiveRepo)
    : IQueryHandler<GetAdaptiveAdjustmentHistoryQuery, ApiResponse<List<AdaptiveAdjustmentLogResponse>>>
{
    public async Task<ApiResponse<List<AdaptiveAdjustmentLogResponse>>> HandleAsync(
        GetAdaptiveAdjustmentHistoryQuery query, CancellationToken cancellationToken = default)
    {
        // Verify the activity belongs to this person's roadmap
        var roadmapActivity = await adaptiveRepo.GetWithConfigAsync(query.PersonRoadmapActivityId, cancellationToken);
        if (roadmapActivity is null)
            return ApiResponse<List<AdaptiveAdjustmentLogResponse>>.NotFound("Actividad del roadmap");

        var logs = await adaptiveRepo.GetAdjustmentHistoryAsync(query.PersonRoadmapActivityId, cancellationToken);
        var dtos = logs.Select(AdaptiveAdjustmentLogResponse.From).ToList();
        return ApiResponse<List<AdaptiveAdjustmentLogResponse>>.SuccessResult(dtos);
    }
}
