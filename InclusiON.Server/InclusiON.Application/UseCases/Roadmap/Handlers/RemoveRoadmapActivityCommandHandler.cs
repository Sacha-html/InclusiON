using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class RemoveRoadmapActivityCommandHandler
        : ICommandHandler<RemoveRoadmapActivityCommand, ApiResponse<object>>
    {
        private readonly IRoadmapRepository _roadmaps;
        private readonly IUnitOfWork        _uow;

        public RemoveRoadmapActivityCommandHandler(IRoadmapRepository roadmaps, IUnitOfWork uow)
        {
            _roadmaps = roadmaps;
            _uow      = uow;
        }

        public async Task<ApiResponse<object>> HandleAsync(
            RemoveRoadmapActivityCommand command, CancellationToken cancellationToken)
        {
            var activity = await _roadmaps.GetActivityByIdAsync(command.ActivityEntryId, cancellationToken);

            if (activity is null)
                return ApiResponse<object>.NotFound("Actividad del roadmap");

            if (await _roadmaps.HasResponsesAsync(command.ActivityEntryId, cancellationToken))
                return ApiResponse<object>.ErrorResult(
                    ErrorCode.InvalidOperation,
                    "No se puede eliminar la actividad del roadmap porque ya posee respuestas o progreso registrado por parte del alumno. Considere archivar o desactivar la actividad si desea evitar nuevas asignaciones.");

            _roadmaps.RemoveActivity(activity);
            await _uow.SaveChangesAsync(cancellationToken);

            return ApiResponse<object>.SuccessResult("Actividad eliminada del roadmap exitosamente.");
        }
    }
}
