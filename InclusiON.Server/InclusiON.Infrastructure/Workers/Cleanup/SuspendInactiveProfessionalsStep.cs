using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;

namespace InclusiON.Workers.Cleanup;

public class SuspendInactiveProfessionalsStep(
    ICommandHandler<SuspendInactiveProfessionalsCommand, ApiResponse<SuspendResult>> handler,
    ILogger<SuspendInactiveProfessionalsStep> logger)
    : ICleanupStep
{
    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await handler.HandleAsync(new SuspendInactiveProfessionalsCommand(), cancellationToken);

        if (result.Data?.SuspendedCount > 0)
            logger.LogInformation("Suspended {Count} inactive professionals", result.Data.SuspendedCount);
        else
            logger.LogInformation("No inactive professionals to suspend");
    }
}
