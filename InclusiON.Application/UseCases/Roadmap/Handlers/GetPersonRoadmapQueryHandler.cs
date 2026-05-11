using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Roadmap.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class GetPersonRoadmapQueryHandler
        : IQueryHandler<GetPersonRoadmapQuery, ApiResponse<RoadmapResponse>>
    {
        private readonly IRoadmapRepository _roadmaps;
        private readonly IEncryptionService _encryption;

        public GetPersonRoadmapQueryHandler(IRoadmapRepository roadmaps, IEncryptionService encryption)
        {
            _roadmaps   = roadmaps;
            _encryption = encryption;
        }

        public async Task<ApiResponse<RoadmapResponse>> HandleAsync(
            GetPersonRoadmapQuery query, CancellationToken cancellationToken)
        {
            var roadmap = await _roadmaps.GetByPersonIdAsync(query.PersonId, cancellationToken);

            if (roadmap is null)
                return ApiResponse<RoadmapResponse>.NotFound("Roadmap");

            var dto = RoadmapMapper.ToResponse(roadmap);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(roadmap.Id.ToString()));
            foreach (var area in dto.Areas)
            {
                area.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(area.Id.ToString()));
                foreach (var activity in area.Activities)
                    activity.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(activity.Id.ToString()));
            }
            return ApiResponse<RoadmapResponse>.SuccessResult(dto);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
