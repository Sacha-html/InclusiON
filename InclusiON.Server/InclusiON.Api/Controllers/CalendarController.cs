using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Requests.Calendar;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Calendar;

namespace InclusiON.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class CalendarController : ControllerBase
    {
        private readonly ICalendarEventsRepository _calendarEventsRepository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IFamilyRepository _familyRepository;
        private readonly IPersonsRepository _personsRepository;
        private readonly IAssignmentsRepository _assignmentsRepository;
        private readonly IHttpContextService _httpContextService;
        private readonly INotificationService _notificationService;
        private readonly IDateTimeProvider _dateTime;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CalendarController> _logger;

        public CalendarController(
            ICalendarEventsRepository calendarEventsRepository,
            IProfessionalsRepository professionalsRepository,
            IFamilyRepository familyRepository,
            IPersonsRepository personsRepository,
            IAssignmentsRepository assignmentsRepository,
            IHttpContextService httpContextService,
            INotificationService notificationService,
            IDateTimeProvider dateTime,
            IUnitOfWork unitOfWork,
            ILogger<CalendarController> logger)
        {
            _calendarEventsRepository = calendarEventsRepository;
            _professionalsRepository  = professionalsRepository;
            _familyRepository         = familyRepository;
            _personsRepository        = personsRepository;
            _assignmentsRepository    = assignmentsRepository;
            _httpContextService       = httpContextService;
            _notificationService      = notificationService;
            _dateTime                 = dateTime;
            _unitOfWork               = unitOfWork;
            _logger                   = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<List<CalendarEventResponse>>>> GetEvents(CancellationToken cancellationToken)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            // Check if professional
            var professional = await _professionalsRepository.GetByUserIdAsync(userId.Value, cancellationToken);
            if (professional != null)
            {
                var events = await _calendarEventsRepository.GetByProfessionalAsync(professional.Id, cancellationToken);
                var dtos = events.Select(CalendarEventResponse.From).ToList();
                return Ok(ApiResponse<List<CalendarEventResponse>>.SuccessResult(dtos));
            }

            // Check if family representative
            var representative = await _familyRepository.GetByUserIdAsync(userId.Value, cancellationToken);
            if (representative != null)
            {
                var students = await _familyRepository.GetLinkedPersonsAsync(userId.Value, cancellationToken);
                var studentIds = students.Select(s => s.Id).ToList();

                var events = await _calendarEventsRepository.GetByStudentIdsAsync(studentIds, cancellationToken);
                var dtos = events.Select(CalendarEventResponse.From).ToList();
                return Ok(ApiResponse<List<CalendarEventResponse>>.SuccessResult(dtos));
            }

            return Ok(ApiResponse<List<CalendarEventResponse>>.SuccessResult(new List<CalendarEventResponse>()));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<CalendarEventResponse>>> SaveEvent(
            [FromBody] SaveCalendarEventRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var userId = _httpContextService.GetCurrentUserId();
                if (userId == null) return Unauthorized();

                var professional = await _professionalsRepository.GetByUserIdAsync(userId.Value, cancellationToken);
                if (professional == null) return Forbid();

                if (string.IsNullOrWhiteSpace(request.Title))
                {
                    return BadRequest(ApiResponse<CalendarEventResponse>.ErrorResult(ErrorCode.ValidationFailed, "El título es obligatorio."));
                }

                if (!DateTime.TryParseExact(request.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var dateVal)
                    && !DateTime.TryParse(request.Date, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateVal))
                {
                    return BadRequest(ApiResponse<CalendarEventResponse>.ErrorResult(ErrorCode.ValidationFailed, "Fecha inválida. Formato esperado: YYYY-MM-DD"));
                }

                if (DateOnly.FromDateTime(dateVal) < _dateTime.Today)
                {
                    return BadRequest(ApiResponse<CalendarEventResponse>.ErrorResult(ErrorCode.ValidationFailed, "No se pueden agendar eventos en fechas pasadas."));
                }

                var targetScope = (request.TargetScope ?? "all").Trim().ToLowerInvariant();
                if (targetScope != "all" && targetScope != "single")
                {
                    return BadRequest(ApiResponse<CalendarEventResponse>.ErrorResult(ErrorCode.ValidationFailed, "Alcance de destinatario inválido."));
                }

                // Garantizar UTC para PostgreSQL
                var utcDate = DateTime.SpecifyKind(dateVal.Date, DateTimeKind.Utc);

                CalendarEvent? ev = null;
                bool isEdit = false;
                string? previousScope = null;
                Guid? previousStudentId = null;

                if (request.Id.HasValue && request.Id.Value != Guid.Empty)
                {
                    ev = await _calendarEventsRepository.GetByIdAsync(request.Id.Value, cancellationToken);
                    if (ev == null) return NotFound();
                    if (ev.CreatedByProfessionalId != professional.Id) return Forbid();

                    isEdit = true;
                    previousScope = ev.TargetScope;
                    previousStudentId = ev.StudentId;
                }

                Guid? studentId = null;
                string? studentName = "Todos los alumnos a cargo";

                if (targetScope == "single")
                {
                    if (!request.StudentId.HasValue || request.StudentId.Value == Guid.Empty)
                    {
                        return BadRequest(ApiResponse<CalendarEventResponse>.ErrorResult(
                            ErrorCode.ValidationFailed,
                            "Debe seleccionar un alumno para una actividad individual."));
                    }

                    var assignment = await _assignmentsRepository.GetAssignmentAsync(professional.Id, request.StudentId.Value, cancellationToken);
                    if (assignment == null || !assignment.IsActive)
                    {
                        return Forbid();
                    }

                    var student = await _personsRepository.GetByIdAsync(request.StudentId.Value, cancellationToken);
                    if (student == null || !student.IsActive)
                    {
                        return NotFound(ApiResponse<CalendarEventResponse>.NotFound("Alumno"));
                    }

                    studentId = student.Id;
                    studentName = $"{student.FirstName} {student.LastName}".Trim();
                }

                if (ev == null)
                {
                    ev = new CalendarEvent
                    {
                        Title                   = request.Title.Trim(),
                        Type                    = request.Type.Trim(),
                        Date                    = utcDate,
                        Time                    = request.Time.Trim(),
                        Description             = request.Description?.Trim(),
                        StudentId               = studentId,
                        StudentName             = studentName,
                        CreatedByProfessionalId = professional.Id,
                        TargetScope             = targetScope
                    };
                    await _calendarEventsRepository.CreateAsync(ev, cancellationToken);
                }
                else
                {
                    ev.Title       = request.Title.Trim();
                    ev.Type        = request.Type.Trim();
                    ev.Date        = utcDate;
                    ev.Time        = request.Time.Trim();
                    ev.Description = request.Description?.Trim();
                    ev.StudentId   = studentId;
                    ev.StudentName = studentName;
                    ev.TargetScope = targetScope;
                    ev.UpdatedAt   = _dateTime.UtcNow;
                    await _calendarEventsRepository.UpdateAsync(ev, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Notificaciones persistentes + tiempo real a todos los tutores activos alcanzados
                await NotifyCalendarEventSavedAsync(
                    professional, ev, isEdit, previousScope, previousStudentId, cancellationToken);

                ev.CreatedByProfessional = professional;
                return Ok(ApiResponse<CalendarEventResponse>.SuccessResult(CalendarEventResponse.From(ev)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al guardar evento de calendario");
                return BadRequest(ApiResponse<CalendarEventResponse>.ErrorResult(ErrorCode.InternalError, $"Error al procesar el evento: {ex.Message}"));
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteEvent(
            Guid id,
            CancellationToken cancellationToken)
        {
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var professional = await _professionalsRepository.GetByUserIdAsync(userId.Value, cancellationToken);
            if (professional == null) return Forbid();

            var ev = await _calendarEventsRepository.GetByIdAsync(id, cancellationToken);
            if (ev == null) return NotFound();
            if (ev.CreatedByProfessionalId != professional.Id) return Forbid();

            await _calendarEventsRepository.DeleteAsync(ev, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await NotifyCalendarEventDeletedAsync(professional, ev, cancellationToken);

            return Ok(ApiResponse<object>.SuccessResult("Evento eliminado exitosamente."));
        }

        private async Task NotifyCalendarEventSavedAsync(
            Professional professional,
            CalendarEvent ev,
            bool isEdit,
            string? previousScope,
            Guid? previousStudentId,
            CancellationToken cancellationToken)
        {
            try
            {
                var professionalName = $"{professional.FirstName} {professional.LastName}".Trim();
                var formattedDate = ev.Date.ToString("dd/MM/yyyy");
                var notifications = new List<NotificationMessage>();

                // Si al editar cambió el alumno objetivo de un evento individual, avisar cancelación a los tutores anteriores
                if (isEdit && previousScope == "single" && previousStudentId.HasValue && previousStudentId != ev.StudentId)
                {
                    var oldTutorIds = await GetActiveTutorUserIdsForStudentAsync(previousStudentId.Value, cancellationToken);
                    foreach (var tutorUserId in oldTutorIds)
                    {
                        notifications.Add(new NotificationMessage(
                            UserId: tutorUserId,
                            Type: NotificationTypes.Calendar,
                            EventCode: NotificationEventCodes.CalendarEventDeleted,
                            Title: $"Evento cancelado: {ev.Type}",
                            Message: $"{professionalName} retiró la {ev.Type} \"{ev.Title}\" del calendario de tu alumno a cargo.",
                            ActionUrl: "/family/calendar"));
                    }
                }

                var isNewForCurrentTarget = !isEdit || (previousScope == "single" && previousStudentId != ev.StudentId);
                var eventCode = isNewForCurrentTarget
                    ? NotificationEventCodes.CalendarEventCreated
                    : NotificationEventCodes.CalendarEventUpdated;

                if (ev.TargetScope == "single" && ev.StudentId.HasValue)
                {
                    // Todos los tutores activos del alumno reciben notificación en Tutoría, Clase y Tarea
                    var tutorIds = await GetActiveTutorUserIdsForStudentAsync(ev.StudentId.Value, cancellationToken);
                    var title = isNewForCurrentTarget
                        ? $"Nuevo evento en calendario: {ev.Type}"
                        : $"Evento reprogramado: {ev.Type}";
                    var verb = isNewForCurrentTarget ? "agendó" : "modificó";
                    var message = $"{professionalName} {verb} una {ev.Type} \"{ev.Title}\" para {ev.StudentName} el {formattedDate} a las {ev.Time} hs.";

                    foreach (var tutorUserId in tutorIds)
                    {
                        notifications.Add(new NotificationMessage(
                            UserId: tutorUserId,
                            Type: NotificationTypes.Calendar,
                            EventCode: eventCode,
                            Title: title,
                            Message: message,
                            ActionUrl: "/family/calendar"));
                    }
                }
                else if (ev.TargetScope == "all")
                {
                    var tutorIds = await GetActiveTutorUserIdsForProfessionalAsync(professional.Id, cancellationToken);
                    var title = isNewForCurrentTarget
                        ? $"Nuevo evento general: {ev.Type}"
                        : $"Evento general reprogramado: {ev.Type}";
                    var verb = isNewForCurrentTarget ? "agendó" : "modificó";
                    var message = $"{professionalName} {verb} una {ev.Type} general \"{ev.Title}\" el {formattedDate} a las {ev.Time} hs.";

                    foreach (var tutorUserId in tutorIds)
                    {
                        notifications.Add(new NotificationMessage(
                            UserId: tutorUserId,
                            Type: NotificationTypes.Calendar,
                            EventCode: eventCode,
                            Title: title,
                            Message: message,
                            ActionUrl: "/family/calendar"));
                    }
                }

                await _notificationService.NotifyManyAsync(notifications, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificaciones de calendario para evento {EventId}", ev.Id);
            }
        }

        private async Task NotifyCalendarEventDeletedAsync(
            Professional professional,
            CalendarEvent ev,
            CancellationToken cancellationToken)
        {
            try
            {
                var professionalName = $"{professional.FirstName} {professional.LastName}".Trim();
                var formattedDate = ev.Date.ToString("dd/MM/yyyy");
                HashSet<Guid> tutorIds;

                if (ev.TargetScope == "single" && ev.StudentId.HasValue)
                {
                    tutorIds = await GetActiveTutorUserIdsForStudentAsync(ev.StudentId.Value, cancellationToken);
                }
                else if (ev.TargetScope == "all")
                {
                    tutorIds = await GetActiveTutorUserIdsForProfessionalAsync(professional.Id, cancellationToken);
                }
                else
                {
                    return;
                }

                var targetLabel = ev.TargetScope == "single" && !string.IsNullOrWhiteSpace(ev.StudentName)
                    ? $" para {ev.StudentName}"
                    : " general";

                var notifications = tutorIds.Select(tutorUserId => new NotificationMessage(
                    UserId: tutorUserId,
                    Type: NotificationTypes.Calendar,
                    EventCode: NotificationEventCodes.CalendarEventDeleted,
                    Title: $"Evento cancelado: {ev.Type}",
                    Message: $"{professionalName} canceló la {ev.Type}{targetLabel} \"{ev.Title}\" programada para el {formattedDate} a las {ev.Time} hs.",
                    ActionUrl: "/family/calendar"));

                await _notificationService.NotifyManyAsync(notifications, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando notificaciones de cancelación para evento {EventId}", ev.Id);
            }
        }

        private async Task<HashSet<Guid>> GetActiveTutorUserIdsForStudentAsync(Guid studentId, CancellationToken cancellationToken)
        {
            var representatives = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(studentId, cancellationToken);
            return representatives
                .Where(r => r.IsActive
                            && r.Representative != null
                            && r.Representative.IsActive
                            && (r.Representative.User == null || r.Representative.User.IsActive)
                            && r.Representative.UserId != Guid.Empty)
                .Select(r => r.Representative.UserId)
                .ToHashSet();
        }

        private async Task<HashSet<Guid>> GetActiveTutorUserIdsForProfessionalAsync(Guid professionalId, CancellationToken cancellationToken)
        {
            var assignments = await _assignmentsRepository.GetPersonsByProfessionalIdAsync(professionalId, cancellationToken);
            var activePersonIds = assignments.Where(a => a.IsActive).Select(a => a.PersonId).Distinct().ToList();
            var tutorUserIds = new HashSet<Guid>();

            foreach (var personId in activePersonIds)
            {
                var ids = await GetActiveTutorUserIdsForStudentAsync(personId, cancellationToken);
                foreach (var id in ids)
                {
                    tutorUserIds.Add(id);
                }
            }

            return tutorUserIds;
        }
    }
}
