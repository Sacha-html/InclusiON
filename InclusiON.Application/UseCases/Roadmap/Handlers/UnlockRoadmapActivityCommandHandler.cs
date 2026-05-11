using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
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
        private readonly IEncryptionService _encryption;

        public UnlockRoadmapActivityCommandHandler(IRoadmapRepository roadmaps, IUnitOfWork uow, IEncryptionService encryption)
        {
            _roadmaps   = roadmaps;
            _uow        = uow;
            _encryption = encryption;
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

            var actDto = RoadmapMapper.ToActivityResponse(activity);
            actDto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(activity.Id.ToString()));
            return ApiResponse<RoadmapActivityResponse>.SuccessResult(actDto, "Actividad desbloqueada exitosamente.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
