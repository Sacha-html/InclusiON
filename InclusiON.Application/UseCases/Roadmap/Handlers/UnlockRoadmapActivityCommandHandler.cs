using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class UnlockRoadmapActivityCommandHandler
        : ICommandHandler<UnlockRoadmapActivityCommand, ApiResponse<RoadmapActivityResponse>>
    {
        private readonly IRoadmapRepository            _roadmaps;
        private readonly IActivityAssignmentRepository _assignments;
        private readonly IUnitOfWork                   _uow;
        private readonly IDateTimeProvider             _dateTime;
        private readonly IEncryptionService            _encryption;

        public UnlockRoadmapActivityCommandHandler(
            IRoadmapRepository            roadmaps,
            IActivityAssignmentRepository assignments,
            IUnitOfWork                   uow,
            IDateTimeProvider             dateTime,
            IEncryptionService            encryption)
        {
            _roadmaps   = roadmaps;
            _assignments = assignments;
            _uow        = uow;
            _dateTime   = dateTime;
            _encryption = encryption;
        }

        public async Task<ApiResponse<RoadmapActivityResponse>> HandleAsync(
            UnlockRoadmapActivityCommand command, CancellationToken cancellationToken)
        {
            var entry = await _roadmaps.GetActivityByIdAsync(command.ActivityEntryId, cancellationToken);

            if (entry is null)
                return ApiResponse<RoadmapActivityResponse>.NotFound("Actividad del roadmap");

            if (entry.IsUnlocked)
                return ApiResponse<RoadmapActivityResponse>.Conflict(
                    ErrorCode.Conflict,
                    "La actividad ya esta desbloqueada.");

            entry.IsUnlocked = true;
            entry.UnlockedAt = _dateTime.UtcNow;

            // Crear la asignación correspondiente para que la persona pueda realizar la actividad
            var assignment = new ActivityAssignment
            {
                ActivityId               = entry.ActivityId,
                PersonId                 = command.PersonId,
                AssignedByProfessionalId = command.ProfessionalId,
                AssignedAt               = _dateTime.UtcNow,
                StatusId                 = AssignmentStatuses.Pendiente,
                CreatedAt                = _dateTime.UtcNow,
            };
            await _assignments.CreateAsync(assignment, cancellationToken);

            await _uow.SaveChangesAsync(cancellationToken);

            var actDto = RoadmapMapper.ToActivityResponse(entry);
            actDto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(entry.Id.ToString()));
            return ApiResponse<RoadmapActivityResponse>.SuccessResult(actDto, "Actividad desbloqueada y asignada exitosamente.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
