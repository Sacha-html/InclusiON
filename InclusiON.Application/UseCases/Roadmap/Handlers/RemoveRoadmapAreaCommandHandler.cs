using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class RemoveRoadmapAreaCommandHandler
        : ICommandHandler<RemoveRoadmapAreaCommand, ApiResponse<object>>
    {
        private readonly IRoadmapRepository _roadmaps;
        private readonly IUnitOfWork        _uow;

        public RemoveRoadmapAreaCommandHandler(IRoadmapRepository roadmaps, IUnitOfWork uow)
        {
            _roadmaps = roadmaps;
            _uow      = uow;
        }

        public async Task<ApiResponse<object>> HandleAsync(
            RemoveRoadmapAreaCommand command, CancellationToken cancellationToken)
        {
            var area = await _roadmaps.GetAreaByIdAsync(command.AreaId, cancellationToken);

            if (area is null)
                return ApiResponse<object>.NotFound("Area del roadmap");

            _roadmaps.RemoveArea(area);
            await _uow.SaveChangesAsync(cancellationToken);

            return ApiResponse<object>.SuccessResult("Area eliminada del roadmap exitosamente.");
        }
    }
}
