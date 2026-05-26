using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Roadmap.Commands;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Roadmap;

namespace InclusiON.Application.UseCases.Roadmap.Handlers
{
    public class AddRoadmapActivityCommandHandler
        : ICommandHandler<AddRoadmapActivityCommand, ApiResponse<RoadmapActivityResponse>>
    {
        private readonly IRoadmapRepository    _roadmaps;
        private readonly IActivitiesRepository _activities;
        private readonly IUnitOfWork           _uow;
        private readonly IEncryptionService    _encryption;

        public AddRoadmapActivityCommandHandler(
            IRoadmapRepository roadmaps,
            IActivitiesRepository activities,
            IUnitOfWork uow,
            IEncryptionService encryption)
        {
            _roadmaps   = roadmaps;
            _activities = activities;
            _uow        = uow;
            _encryption = encryption;
        }

        public async Task<ApiResponse<RoadmapActivityResponse>> HandleAsync(
            AddRoadmapActivityCommand command, CancellationToken cancellationToken)
        {
            // 1. Verificar que el area existe en el roadmap
            var area = await _roadmaps.GetAreaByIdAsync(command.AreaId, cancellationToken);
            if (area is null)
                return ApiResponse<RoadmapActivityResponse>.NotFound("Area del roadmap");

            // 2. Verificar que la actividad existe y esta activa
            var activity = await _activities.GetByIdAsync(command.ActivityId, cancellationToken);
            if (activity is null || !activity.IsActive)
                return ApiResponse<RoadmapActivityResponse>.NotFound("Actividad");

            // 2b. Verificar que el profesional puede usar la actividad (propia o estandar)
            if (!activity.IsStandardActivity && activity.ProfessionalId != command.ProfessionalId)
                return ApiResponse<RoadmapActivityResponse>.Forbidden();

            // 2c. Verificar que la actividad pertenece al mismo SkillArea que el área del roadmap
            if (activity.SkillAreaId.HasValue && activity.SkillAreaId.Value != area.SkillAreaId)
                return ApiResponse<RoadmapActivityResponse>.ErrorResult(
                    ErrorCode.InvalidOperation,
                    "La actividad no pertenece al área de habilidad de este tramo del roadmap.");

            // 3. Verificar que la actividad no este ya en el area
            if (await _roadmaps.ActivityExistsInAreaAsync(command.AreaId, command.ActivityId, cancellationToken))
                return ApiResponse<RoadmapActivityResponse>.Conflict(
                    ErrorCode.Conflict,
                    "La actividad ya esta asignada en esta area del roadmap.");

            // 4. La primera actividad (SequenceOrder == 1) se desbloquea automaticamente
            var isFirst  = command.SequenceOrder == 1;

            var entry = new PersonRoadmapActivity
            {
                PersonRoadmapAreaId    = command.AreaId,
                ActivityId             = command.ActivityId,
                SequenceOrder          = command.SequenceOrder,
                IsUnlocked             = isFirst,
                UnlockedAt             = isFirst ? DateTime.UtcNow : null,
                UnlockThresholdPercent = command.UnlockThresholdPercent,
                TimeLimitSeconds       = command.TimeLimitSeconds,
                MaxAttempts            = command.MaxAttempts,
                ShowHints              = command.ShowHints,
                DifficultyLevel        = command.DifficultyLevel,
                // No asignar Activity — viene de AsNoTracking y EF la marcaría Added causando PK duplicado
            };

            await _roadmaps.AddActivityAsync(entry, cancellationToken);
            await _uow.SaveChangesAsync(cancellationToken);

            // Asignar después del save solo para el mapeo de la respuesta
            entry.Activity = activity;

            var actDto = RoadmapMapper.ToActivityResponse(entry);
            actDto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(entry.Id.ToString()));
            return ApiResponse<RoadmapActivityResponse>.SuccessResult(actDto,
                "Actividad agregada al roadmap exitosamente.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
