using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;

namespace InclusiON.Agents.Cleanup;

public class SuspendInactiveProfessionalsStep : ICleanupStep
{
    readonly ICommandHandler<SuspendInactiveProfessionalsCommand, ApiResponse<SuspendResult>> _handler;
    readonly ILogger<SuspendInactiveProfessionalsStep> _logger;

    public SuspendInactiveProfessionalsStep(
        ICommandHandler<SuspendInactiveProfessionalsCommand, ApiResponse<SuspendResult>> handler,
        ILogger<SuspendInactiveProfessionalsStep> logger)
    {
        _handler = handler;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var result = await _handler.HandleAsync(new SuspendInactiveProfessionalsCommand(), cancellationToken);

        if (result.Data?.SuspendedCount > 0)
            _logger.LogInformation("Suspended {Count} inactive professionals", result.Data.SuspendedCount);
        else
            _logger.LogInformation("No inactive professionals to suspend");
    }
}
