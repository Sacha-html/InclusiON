using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;

namespace InclusiON.Application.UseCases.Roadmap.Handlers;

public class DeleteAdaptiveEngineConfigCommandHandler(IAdaptiveEngineRepository adaptiveRepo)
    : ICommandHandler<DeleteAdaptiveEngineConfigCommand, ApiResponse<object>>
{
    public async Task<ApiResponse<object>> HandleAsync(
        DeleteAdaptiveEngineConfigCommand command, CancellationToken cancellationToken = default)
    {
        await adaptiveRepo.DeleteConfigAsync(command.PersonRoadmapActivityId, cancellationToken);
        return ApiResponse<object>.SuccessResult(null!, "Motor adaptativo deshabilitado.");
    }
}
