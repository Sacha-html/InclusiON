using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
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

        public UpdateRoadmapNotesCommandHandler(IRoadmapRepository roadmaps, IUnitOfWork uow)
        {
            _roadmaps = roadmaps;
            _uow      = uow;
        }

        public async Task<ApiResponse<RoadmapResponse>> HandleAsync(
            UpdateRoadmapNotesCommand command, CancellationToken cancellationToken)
        {
            var roadmap = await _roadmaps.GetByPersonIdAsync(command.PersonId, cancellationToken);

            if (roadmap is null)
                return ApiResponse<RoadmapResponse>.NotFound("Roadmap");

            roadmap.Notes = command.Notes;
            await _uow.SaveChangesAsync(cancellationToken);

            return ApiResponse<RoadmapResponse>.SuccessResult(
                GetPersonRoadmapQueryHandler.Map(roadmap),
                "Notas del roadmap actualizadas exitosamente.");
        }
    }
}
