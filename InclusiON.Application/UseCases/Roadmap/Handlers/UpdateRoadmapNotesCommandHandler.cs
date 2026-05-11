using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class UpdateRoadmapNotesCommandHandler
        : ICommandHandler<UpdateRoadmapNotesCommand, ApiResponse<RoadmapResponse>>
    {
        private readonly IRoadmapRepository _roadmaps;
        private readonly IUnitOfWork        _uow;
        private readonly IEncryptionService _encryption;

        public UpdateRoadmapNotesCommandHandler(IRoadmapRepository roadmaps, IUnitOfWork uow, IEncryptionService encryption)
        {
            _roadmaps   = roadmaps;
            _uow        = uow;
            _encryption = encryption;
        }

        public async Task<ApiResponse<RoadmapResponse>> HandleAsync(
            UpdateRoadmapNotesCommand command, CancellationToken cancellationToken)
        {
            var roadmap = await _roadmaps.GetByPersonIdAsync(command.PersonId, cancellationToken);

            if (roadmap is null)
                return ApiResponse<RoadmapResponse>.NotFound("Roadmap");

            roadmap.Notes = command.Notes;
            await _uow.SaveChangesAsync(cancellationToken);

            var dto = RoadmapMapper.ToResponse(roadmap);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(roadmap.Id.ToString()));
            foreach (var area in dto.Areas)
            {
                area.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(area.Id.ToString()));
                foreach (var activity in area.Activities)
                    activity.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(activity.Id.ToString()));
            }
            return ApiResponse<RoadmapResponse>.SuccessResult(dto, "Notas del roadmap actualizadas exitosamente.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
