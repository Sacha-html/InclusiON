using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class UnlockRoadmapActivityCommandHandler
        : ICommandHandler<UnlockRoadmapActivityCommand, ApiResponse<RoadmapActivityResponse>>
    {
        private readonly IRoadmapRepository _roadmaps;
        private readonly IUnitOfWork        _uow;

        public UnlockRoadmapActivityCommandHandler(IRoadmapRepository roadmaps, IUnitOfWork uow)
        {
            _roadmaps = roadmaps;
            _uow      = uow;
        }

        public async Task<ApiResponse<RoadmapActivityResponse>> HandleAsync(
            UnlockRoadmapActivityCommand command, CancellationToken cancellationToken)
        {
            var activity = await _roadmaps.GetActivityByIdAsync(command.ActivityEntryId, cancellationToken);

            if (activity is null)
                return ApiResponse<RoadmapActivityResponse>.NotFound("Actividad del roadmap");

            if (activity.IsUnlocked)
                return ApiResponse<RoadmapActivityResponse>.Conflict(
                    ErrorCode.Conflict,
                    "La actividad ya esta desbloqueada.");

            activity.IsUnlocked = true;
            activity.UnlockedAt = DateTime.UtcNow;

            await _uow.SaveChangesAsync(cancellationToken);

            return ApiResponse<RoadmapActivityResponse>.SuccessResult(
                GetPersonRoadmapQueryHandler.MapActivity(activity),
                "Actividad desbloqueada exitosamente.");
        }
    }
}
