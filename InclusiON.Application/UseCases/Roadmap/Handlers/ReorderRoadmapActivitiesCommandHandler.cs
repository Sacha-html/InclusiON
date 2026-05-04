using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class ReorderRoadmapActivitiesCommandHandler
        : ICommandHandler<ReorderRoadmapActivitiesCommand, ApiResponse<object>>
    {
        private readonly IRoadmapRepository _roadmaps;
        private readonly IUnitOfWork        _uow;

        public ReorderRoadmapActivitiesCommandHandler(IRoadmapRepository roadmaps, IUnitOfWork uow)
        {
            _roadmaps = roadmaps;
            _uow      = uow;
        }

        public async Task<ApiResponse<object>> HandleAsync(
            ReorderRoadmapActivitiesCommand command, CancellationToken cancellationToken)
        {
            var activities = await _roadmaps.GetActivitiesByAreaIdAsync(command.AreaId, cancellationToken);

            if (activities.Count == 0)
                return ApiResponse<object>.NotFound("Actividades del área");

            // Validate every provided ID belongs to this area
            var areaIds = activities.Select(a => a.Id).ToHashSet();
            var invalid = command.Activities
                .Where(item => !areaIds.Contains(item.Id))
                .ToList();

            if (invalid.Count > 0)
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    "Una o más actividades no pertenecen a este área.");

            foreach (var item in command.Activities)
            {
                var activity = activities.First(a => a.Id == item.Id);
                activity.SequenceOrder = item.SequenceOrder;
            }

            await _uow.SaveChangesAsync(cancellationToken);

            return ApiResponse<object>.SuccessResult(null!, "Actividades reordenadas.");
        }
    }
}
