using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers;

public class GetSkillRadarQueryHandler(IAdaptiveEngineRepository adaptiveRepo)
    : IQueryHandler<GetSkillRadarQuery, ApiResponse<List<SkillRadarPointResponse>>>
{
    public async Task<ApiResponse<List<SkillRadarPointResponse>>> HandleAsync(
        GetSkillRadarQuery query, CancellationToken cancellationToken = default)
    {
        var points = await adaptiveRepo.GetSkillRadarAsync(query.PersonId, cancellationToken);
        return ApiResponse<List<SkillRadarPointResponse>>.SuccessResult(points);
    }
}
