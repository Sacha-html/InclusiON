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
        private readonly IRealTimeNotifier? _realTimeNotifier;
        private readonly IFamilyRepository? _familyRepository;

        public CompleteActivityResponseCommandHandler(
            IActivityAssignmentRepository repository,
            IRoadmapRepository roadmapRepository,
            IProfessionalsRepository professionalsRepository,
            IBackgroundJobRepository backgroundJobs,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEncryptionService encryption,
            IRealTimeNotifier? realTimeNotifier = null,
            IFamilyRepository? familyRepository = null)
        {
            _repository              = repository;
            _roadmapRepository       = roadmapRepository;
            _professionalsRepository = professionalsRepository;
            _backgroundJobs          = backgroundJobs;
            _unitOfWork              = unitOfWork;
            _dateTime                = dateTime;
            _encryption              = encryption;
            _realTimeNotifier        = realTimeNotifier;
            _familyRepository        = familyRepository;
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

            // Calcular puntuación clínica Goal Attainment Scaling (GAS [-2, +2])
            int gasScore;
            if (command.SuccessPercentage >= 81m)
                gasScore = 2;
            else if (command.SuccessPercentage >= 70m)
                gasScore = 1;
            else if (command.SuccessPercentage >= 60m)
                gasScore = 0;
            else if (command.SuccessPercentage >= 31m)
                gasScore = -1;
            else
                gasScore = -2;

            if (response.AttemptCount >= 4 && command.SuccessPercentage < 60m)
            {
                gasScore = -2;
                if (!command.FrustrationLevel.HasValue)
                {
                    response.FrustrationLevel = 4;
                }
            }

            // Read roadmap data before any mutations — both reads only need PersonId/ActivityId
            // which are available from the already-loaded assignment.
            var roadmapEntry = await _roadmapRepository.GetByPersonAndActivityAsync(
                assignment.PersonId, assignment.ActivityId, cancellationToken);

            PersonRoadmapActivity? nextToUnlock = null;
            // Desbloqueo del siguiente nivel: GAS >= 0 (éxito >= 60%) y cumple umbral configurado
            if (roadmapEntry is not null && gasScore >= 0 && command.SuccessPercentage >= roadmapEntry.UnlockThresholdPercent)
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
            response.FrustrationLevel  = command.FrustrationLevel ?? response.FrustrationLevel;
            response.ResponsePattern   = command.ResponsePattern;
            response.Observations      = command.Observations;
            response.UpdatedAt         = now;

            await _repository.UpdateResponseAsync(response, cancellationToken);

            if (command.SuccessPercentage >= 60m)
            {
                assignment.StatusId  = AssignmentStatuses.Completada;
            }
            else
            {
                assignment.StatusId  = AssignmentStatuses.EnProgreso;
            }
            assignment.UpdatedAt = now;
            await _repository.UpdateAsync(assignment, cancellationToken);

            // Sincronizar persistencia en la entidad analítica ActivitySession
            int errorCount = 0;
            if (command.SuccessPercentage <= 30m || response.AttemptCount >= 4)
                errorCount = Math.Max(4, response.AttemptCount);
            else if (command.SuccessPercentage < 60m)
                errorCount = Math.Max(2, response.AttemptCount);
            else if (command.SuccessPercentage < 80m)
                errorCount = 1;

            var session = new ActivitySession
            {
                StudentId        = assignment.PersonId,
                ProfessionalId   = assignment.AssignedByProfessionalId,
                ActivityId       = assignment.ActivityId,
                DateCompleted    = now,
                TimeSpentSeconds = command.TimeSpentSeconds,
                SuccessRate      = command.SuccessPercentage,
                ErrorCount       = errorCount,
                GasScore         = gasScore,
                IsActive         = true,
                CreatedAt        = now,
            };

            await _repository.CreateSessionAsync(session, cancellationToken);

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

            // Ejecutar la creación de los trabajos de fondo de forma secuencial y segura
            try
            {
                var prof = await _professionalsRepository.GetByIdAsync(
                    assignment.AssignedByProfessionalId, cancellationToken);
                var professionalUserId = prof?.UserId.ToString() ?? string.Empty;

                // Ajuste adaptativo - solo si la actividad está en el roadmap
                if (roadmapEntry?.Id > 0)
                {
                    await _backgroundJobs.CreateAsync(
                        JobTypes.AdaptiveAdjustment,
                        JsonSerializer.Serialize(new
                        {
                            PersonRoadmapActivityId = roadmapEntry.Id,
                            ActivityResponseId      = response.Id,
                            AssignmentId            = assignment.Id,
                            ProfessionalUserId      = professionalUserId
                        }),
                        maxRetries: 3,
                        cancellationToken: cancellationToken);
                }

                // Notificar al profesional
                if (prof is not null)
                {
                    var studentName = assignment.Person != null ? $"{assignment.Person.FirstName} {assignment.Person.LastName}" : "Un alumno";
                    var activityTitle = assignment.Activity != null ? assignment.Activity.Title : "una actividad";

                    string notifTitle;
                    string notifMessage;

                    if (command.SuccessPercentage >= 60m)
                    {
                        notifTitle = "Actividad completada";
                        notifMessage = $"{studentName} completó la actividad '{activityTitle}' con éxito ({command.SuccessPercentage:F0}% de logro).";
                    }
                    else if (response.AttemptCount >= 4)
                    {
                        notifTitle = "Alerta: Apoyo pedagógico requerido (4 intentos)";
                        notifMessage = $"{studentName} alcanzó 4 intentos en la actividad '{activityTitle}' (último intento: {command.SuccessPercentage:F0}%). La actividad continúa disponible pero requiere tu intervención.";
                    }
                    else
                    {
                        notifTitle = "Intento de actividad registrado";
                        notifMessage = $"{studentName} realizó el intento {response.AttemptCount} en la actividad '{activityTitle}' ({command.SuccessPercentage:F0}% de éxito). El nivel continúa en proceso.";
                    }

                    string actionUrl;
                    if (response.AttemptCount >= 4 && command.SuccessPercentage < 60m)
                    {
                        actionUrl = $"/pro/evaluations?personId={assignment.PersonId}&activityId={assignment.ActivityId}&alert=attempts";
                    }
                    else
                    {
                        actionUrl = $"/pro/evaluations?personId={assignment.PersonId}";
                    }

                    var payload = new NotificationPayload
                    {
                        UserId    = professionalUserId,
                        Title     = notifTitle,
                        Message   = notifMessage,
                        ActionUrl = actionUrl
                    };

                    if (!string.IsNullOrEmpty(professionalUserId) && _realTimeNotifier != null)
                    {
                        try
                        {
                            await _realTimeNotifier.NotifyUserAsync(
                                professionalUserId,
                                notifTitle,
                                notifMessage,
                                payload.ActionUrl,
                                cancellationToken);
                        }
                        catch
                        {
                            // Continuar con BackgroundJob
                        }
                    }

                    await _backgroundJobs.CreateAsync(
                        JobTypes.Push,
                        JsonSerializer.Serialize(payload),
                        maxRetries: 3,
                        cancellationToken: cancellationToken);
                }

                // Notificar a los tutores familiares vinculados al alumno
                if (_familyRepository is not null)
                {
                    var studentName = assignment.Person != null ? $"{assignment.Person.FirstName} {assignment.Person.LastName}".Trim() : "El estudiante";
                    var activityTitle = assignment.Activity != null ? assignment.Activity.Title : "la actividad";

                    var reps = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(assignment.PersonId, cancellationToken);
                    foreach (var rep in reps.Where(r => r.IsActive && r.Representative != null && r.Representative.UserId != Guid.Empty))
                    {
                        var tutorUserId = rep.Representative!.UserId.ToString();
                        string tutorTitle;
                        string tutorMessage;
                        string notifType;

                        if (command.SuccessPercentage >= 60m)
                        {
                            tutorTitle = "¡Felicitaciones! Nivel superado";
                            tutorMessage = $"{studentName} completó la actividad '{activityTitle}' con éxito ({command.SuccessPercentage:F0}% de logro) en Mi Camino.";
                            notifType = "ActivityCompleted";
                        }
                        else if (response.AttemptCount >= 4)
                        {
                            tutorTitle = "Alerta en Mi Camino: Apoyo requerido";
                            tutorMessage = $"{studentName} registró varios intentos en la actividad '{activityTitle}'. Su profesional ha sido notificado para asistirlo.";
                            notifType = "ActivityBlocked";
                        }
                        else
                        {
                            tutorTitle = "Nuevo intento registrado";
                            tutorMessage = $"{studentName} realizó un intento en '{activityTitle}' ({command.SuccessPercentage:F0}% de logro).";
                            notifType = "ActivityAttempt";
                        }

                        if (_realTimeNotifier != null)
                        {
                            try
                            {
                                await _realTimeNotifier.NotifyUserAsync(
                                    tutorUserId,
                                    tutorTitle,
                                    tutorMessage,
                                    "/family/dashboard",
                                    cancellationToken);
                            }
                            catch
                            {
                                // Silencioso
                            }
                        }

                        var tutorPayload = new NotificationPayload
                        {
                            UserId    = tutorUserId,
                            Title     = tutorTitle,
                            Message   = tutorMessage,
                            ActionUrl = "/family/dashboard"
                        };

                        await _backgroundJobs.CreateAsync(
                            JobTypes.Push,
                            JsonSerializer.Serialize(tutorPayload),
                            maxRetries: 3,
                            cancellationToken: cancellationToken);
                    }
                }
            }
            catch (Exception)
            {
                // Silent catch para asegurar la robustez de la respuesta principal
            }

            var updated = await _repository.GetByIdAsync(command.AssignmentId, cancellationToken);

            var dto = ActivityAssignmentResponse.From(updated!);
            dto.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(updated!.Id.ToString()));
            foreach (var attempt in dto.Responses)
                attempt.EncryptedId = ToUrlSafeBase64(_encryption.Encrypt(attempt.Id.ToString()));

            string resultMessage = command.SuccessPercentage >= 60m
                ? "Actividad completada."
                : response.AttemptCount >= 4
                    ? "Se han agotado los 4 intentos. Actividad bloqueada."
                    : "Intento registrado. Nivel en proceso.";

            return ApiResponse<ActivityAssignmentResponse>.SuccessResult(dto, resultMessage);
        }

        private static string ToUrlSafeBase64(string s) => s.Replace('+', '-').Replace('/', '_').TrimEnd('=');

        private static ActivityResponseResult ResolveResult(decimal successPercentage) =>
            successPercentage >= 80 ? ActivityResponseResult.Exito
            : successPercentage >= 50 ? ActivityResponseResult.Parcial
            : ActivityResponseResult.Fallido;
    }
}
