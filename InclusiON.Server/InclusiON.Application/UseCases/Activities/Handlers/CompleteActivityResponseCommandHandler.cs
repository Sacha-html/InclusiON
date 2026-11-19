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
        private readonly INotificationService? _notificationService;

        public CompleteActivityResponseCommandHandler(
            IActivityAssignmentRepository repository,
            IRoadmapRepository roadmapRepository,
            IProfessionalsRepository professionalsRepository,
            IBackgroundJobRepository backgroundJobs,
            IUnitOfWork unitOfWork,
            IDateTimeProvider dateTime,
            IEncryptionService encryption,
            IRealTimeNotifier? realTimeNotifier = null,
            IFamilyRepository? familyRepository = null,
            INotificationService? notificationService = null)
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
            _notificationService     = notificationService;
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
            // Regla CB-07 / HU-21: 4 intentos fallidos en la misma actividad = 1 alerta de frustración
            int errorCount;
            if (command.SuccessPercentage >= 60m)
            {
                // Aprobado: No es frustración. Errores mínimos del intento exitoso (0 si >= 80%, 1 si 60-79%)
                errorCount = command.SuccessPercentage >= 80m ? 0 : 1;
            }
            else
            {
                // Fallido (< 60%): Registra el número de intento alcanzado.
                // Si response.AttemptCount >= 4, errorCount >= 4 activará la alerta pedagógica de frustración (CB-07).
                errorCount = Math.Max(1, response.AttemptCount);
            }

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

                // Solo notificar en eventos Core: éxito (>= 60%) o alerta pedagógica CB-07 (4.° intento fallido)
                var isSuccess = command.SuccessPercentage >= 60m;
                var isAttemptAlert = !isSuccess && response.AttemptCount >= 4;

                if (isSuccess || isAttemptAlert)
                {
                    var notifications = new List<NotificationMessage>();
                    var studentName = assignment.Person != null
                        ? $"{assignment.Person.FirstName} {assignment.Person.LastName}".Trim()
                        : "Un alumno";
                    var activityTitle = assignment.Activity != null
                        ? assignment.Activity.Title
                        : "una actividad";
                    var encryptedPersonId = ToUrlSafeBase64(_encryption.Encrypt(assignment.PersonId.ToString()));

                    // Notificar al profesional asignado
                    if (prof is not null && prof.UserId != Guid.Empty)
                    {
                        var profTitle = isSuccess
                            ? "Actividad completada"
                            : "Alerta: Apoyo pedagógico requerido (4 intentos)";
                        var profMessage = isSuccess
                            ? $"{studentName} completó la actividad \"{activityTitle}\" con éxito ({command.SuccessPercentage:F0}% de logro)."
                            : $"{studentName} alcanzó {response.AttemptCount} intentos en la actividad \"{activityTitle}\" (último intento: {command.SuccessPercentage:F0}%). Requiere tu intervención.";
                        var profActionUrl = isAttemptAlert
                            ? $"/pro/evaluations?personId={encryptedPersonId}&activityId={assignment.ActivityId}&alert=attempts"
                            : $"/pro/evaluations?personId={encryptedPersonId}";

                        notifications.Add(new NotificationMessage(
                            UserId: prof.UserId,
                            Type: isAttemptAlert ? NotificationTypes.Alert : NotificationTypes.Activity,
                            EventCode: isAttemptAlert ? NotificationEventCodes.ActivityAttemptAlert : NotificationEventCodes.ActivityCompleted,
                            Title: profTitle,
                            Message: profMessage,
                            ActionUrl: profActionUrl));
                    }

                    // Notificar a todos los tutores familiares activos vinculados al alumno
                    if (_familyRepository is not null)
                    {
                        var reps = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(assignment.PersonId, cancellationToken);
                        var tutorTitle = isSuccess
                            ? "¡Felicitaciones! Nivel superado"
                            : "Alerta en Mi Camino: Apoyo requerido";
                        var tutorMessage = isSuccess
                            ? $"{studentName} completó la actividad \"{activityTitle}\" con éxito ({command.SuccessPercentage:F0}% de logro) en Mi Camino."
                            : $"{studentName} registró varios intentos en la actividad \"{activityTitle}\". Su profesional fue notificado para asistirlo.";

                        foreach (var rep in reps.Where(r => r.IsActive && r.Representative is not null && r.Representative.IsActive && r.Representative.UserId != Guid.Empty))
                        {
                            notifications.Add(new NotificationMessage(
                                UserId: rep.Representative.UserId,
                                Type: isAttemptAlert ? NotificationTypes.Alert : NotificationTypes.Activity,
                                EventCode: isAttemptAlert ? NotificationEventCodes.ActivityAttemptAlert : NotificationEventCodes.ActivityCompleted,
                                Title: tutorTitle,
                                Message: tutorMessage,
                                ActionUrl: "/family"));
                        }
                    }

                    if (_notificationService is not null)
                    {
                        await _notificationService.NotifyManyAsync(notifications, cancellationToken);
                    }
                    else if (_realTimeNotifier is not null)
                    {
                        foreach (var n in notifications)
                        {
                            await _realTimeNotifier.NotifyUserAsync(
                                n.UserId.ToString(), n.Title, n.Message, n.ActionUrl, cancellationToken);
                        }
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
                    ? "Intento registrado. Tu profesional fue notificado para brindarte apoyo."
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
