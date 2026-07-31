using System.Text.Json;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Activities;

namespace InclusiON.Application.UseCases.Activities.Handlers
{
    public class CompleteActivityResponseCommandHandler
        : ICommandHandler<CompleteActivityResponseCommand, ApiResponse<ActivityAssignmentResponse>>
    {
        private readonly IActivityAssignmentRepository _repository;
        private readonly IRoadmapRepository _roadmapRepository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IDateTimeProvider _dateTime;
        private readonly IEncryptionService _encryption;

        public CompleteActivityResponseCommandHandler(
            IActivityAssignmentRepository repository,
            IRoadmapRepository roadmapRepository,
            IProfessionalsRepository professionalsRepository,
            IBackgroundJobRepository backgroundJobs,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEncryptionService encryption)
        {
            _repository              = repository;
            _roadmapRepository       = roadmapRepository;
            _professionalsRepository = professionalsRepository;
            _backgroundJobs          = backgroundJobs;
            _unitOfWork              = unitOfWork;
            _dateTime                = dateTime;
            _encryption              = encryption;
        }

        public async Task<ApiResponse<ActivityAssignmentResponse>> HandleAsync(
            CompleteActivityResponseCommand command, CancellationToken cancellationToken)
        {
            var assignment = await _repository.GetByIdAsync(command.AssignmentId, cancellationToken);

            if (assignment is null)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Asignación");

            if (assignment.PersonId != command.PersonId)
                return ApiResponse<ActivityAssignmentResponse>.Forbidden();

            var response = await _repository.GetResponseByIdAsync(command.ResponseId, cancellationToken);

            if (response is null || response.AssignmentId != command.AssignmentId)
                return ApiResponse<ActivityAssignmentResponse>.NotFound("Respuesta");

            if (response.CompletedAt.HasValue)
                return ApiResponse<ActivityAssignmentResponse>.Conflict(
                    ErrorCode.BusinessRuleViolation,
                    "Esta respuesta ya fue completada.");

            if (command.SuccessPercentage < 0 || command.SuccessPercentage > 100)
                return ApiResponse<ActivityAssignmentResponse>.ErrorResult(
                    ErrorCode.InvalidFormat,
                    "El porcentaje de éxito debe estar entre 0 y 100.");

            if (command.TimeSpentSeconds < 0)
                return ApiResponse<ActivityAssignmentResponse>.ErrorResult(
                    ErrorCode.InvalidFormat,
                    "El tiempo transcurrido no puede ser negativo.");

            if (command.FrustrationLevel.HasValue && (command.FrustrationLevel.Value < 1 || command.FrustrationLevel.Value > 5))
                return ApiResponse<ActivityAssignmentResponse>.ErrorResult(
                    ErrorCode.InvalidFormat,
                    "El nivel de frustración debe estar entre 1 y 5.");

            var now = _dateTime.UtcNow;

            // Read roadmap data before any mutations — both reads only need PersonId/ActivityId
            // which are available from the already-loaded assignment.
            var roadmapEntry = await _roadmapRepository.GetByPersonAndActivityAsync(
                assignment.PersonId, assignment.ActivityId, cancellationToken);

            PersonRoadmapActivity? nextToUnlock = null;
            if (roadmapEntry is not null && command.SuccessPercentage >= roadmapEntry.UnlockThresholdPercent)
            {
                var next = await _roadmapRepository.GetNextInAreaAsync(
                    roadmapEntry.PersonRoadmapAreaId, roadmapEntry.SequenceOrder, cancellationToken);

                if (next is not null && !next.IsUnlocked)
                    nextToUnlock = next;
            }

            // Apply all mutations
            response.CompletedAt       = now;
            response.TimeSpentSeconds  = command.TimeSpentSeconds;
            response.SuccessPercentage = command.SuccessPercentage;
            response.Result            = ResolveResult(command.SuccessPercentage);
            response.RequiredSupport   = command.RequiredSupport;
            response.FrustrationLevel  = command.FrustrationLevel;
            response.ResponsePattern   = command.ResponsePattern;
            response.Observations      = command.Observations;
            response.UpdatedAt         = now;

            await _repository.UpdateResponseAsync(response, cancellationToken);

            assignment.StatusId  = AssignmentStatuses.Completada;
            assignment.UpdatedAt = now;
            await _repository.UpdateAsync(assignment, cancellationToken);

            if (nextToUnlock is not null)
            {
                nextToUnlock.IsUnlocked = true;
                nextToUnlock.UnlockedAt = now;

                var nextAssignment = new ActivityAssignment
                {
                    ActivityId               = nextToUnlock.ActivityId,
                    PersonId                 = assignment.PersonId,
                    AssignedByProfessionalId = assignment.AssignedByProfessionalId,
                    AssignedAt               = now,
                    StatusId                 = AssignmentStatuses.Pendiente,
                    CreatedAt                = now,
                };
                await _repository.CreateAsync(nextAssignment, cancellationToken);
            }

            // Single save — all mutations committed atomically
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Ajuste adaptativo — fire and forget, solo si la actividad esta en roadmap
            if (roadmapEntry?.Id > 0)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var prof = await _professionalsRepository.GetByIdAsync(
                            assignment.AssignedByProfessionalId, CancellationToken.None);
                        await _backgroundJobs.CreateAsync(
                            JobTypes.AdaptiveAdjustment,
                            JsonSerializer.Serialize(new
                            {
                                PersonRoadmapActivityId = roadmapEntry.Id,
                                ActivityResponseId      = response.Id,
                                AssignmentId            = assignment.Id,
                                ProfessionalUserId      = prof?.UserId.ToString() ?? string.Empty
                            }),
                            maxRetries: 3);
                    }
                    catch { /* fire and forget */ }
                });
            }

            // Notificar al profesional — fire and forget
            _ = Task.Run(async () =>
            {
                try
                {
                    var prof = await _professionalsRepository.GetByIdAsync(
                        assignment.AssignedByProfessionalId, CancellationToken.None);
                    if (prof is not null)
                    {
                        await _backgroundJobs.CreateAsync(
                            JobTypes.Push,
                            JsonSerializer.Serialize(new NotificationPayload
                            {
                                UserId    = prof.UserId.ToString(),
                                Title     = "Actividad completada",
                                Message   = $"Una persona completó una actividad asignada por vos ({command.SuccessPercentage:F0}% de éxito).",
                                ActionUrl = "/#/pro/persons"
                            }),
                            maxRetries: 3);
                    }
                }
                catch { /* fire and forget */ }
            });

            var updated = await _repository.GetByIdAsync(command.AssignmentId, cancellationToken);

            var dto = ActivityAssignmentResponse.From(updated!);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(updated!.Id.ToString()));
            foreach (var attempt in dto.Responses)
                attempt.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(attempt.Id.ToString()));
            return ApiResponse<ActivityAssignmentResponse>.SuccessResult(dto, "Actividad completada.");
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');

        private static ActivityResponseResult ResolveResult(decimal successPercentage) =>
            successPercentage >= 80 ? ActivityResponseResult.Exito
            : successPercentage >= 50 ? ActivityResponseResult.Parcial
            : ActivityResponseResult.Fallido;
    }
}
