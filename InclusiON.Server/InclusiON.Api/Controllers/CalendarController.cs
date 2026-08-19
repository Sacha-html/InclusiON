using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Models;
using InclusiON.Domain.Enums;
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
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly IUnitOfWork _unitOfWork;

        public CalendarController(
            ICalendarEventsRepository calendarEventsRepository,
            IProfessionalsRepository professionalsRepository,
            IFamilyRepository familyRepository,
            IPersonsRepository personsRepository,
            IAssignmentsRepository assignmentsRepository,
            IHttpContextService httpContextService,
            IBackgroundJobRepository backgroundJobs,
            IUnitOfWork unitOfWork)
        {
            _calendarEventsRepository = calendarEventsRepository;
            _professionalsRepository = professionalsRepository;
            _familyRepository = familyRepository;
            _personsRepository = personsRepository;
            _assignmentsRepository = assignmentsRepository;
            _httpContextService = httpContextService;
            _backgroundJobs = backgroundJobs;
            _unitOfWork = unitOfWork;
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

                if (!DateTime.TryParse(request.Date, out var dateVal))
                {
                    return BadRequest(ApiResponse<CalendarEventResponse>.ErrorResult(ErrorCode.ValidationFailed, "Fecha inválida. Formato esperado: YYYY-MM-DD"));
                }

                // Garantizar UTC para PostgreSQL
                var utcDate = DateTime.SpecifyKind(dateVal.Date, DateTimeKind.Utc);

                CalendarEvent? ev = null;
                if (!string.IsNullOrEmpty(request.Id) && Guid.TryParse(request.Id, out var eventId))
                {
                    ev = await _calendarEventsRepository.GetByIdAsync(eventId, cancellationToken);
                    if (ev == null) return NotFound();
                    if (ev.CreatedByProfessionalId != professional.Id) return Forbid();
                }

                Guid? studentId = null;
                string? studentName = "Todos los alumnos a cargo";

                if (request.TargetScope == "single" && !string.IsNullOrEmpty(request.StudentId))
                {
                    if (Guid.TryParse(request.StudentId, out var parsedStudentId))
                    {
                        studentId = parsedStudentId;
                        var student = await _personsRepository.GetByIdAsync(parsedStudentId, cancellationToken);
                        if (student != null)
                        {
                            studentName = $"{student.FirstName} {student.LastName}";
                        }
                    }
                }

                if (ev == null)
                {
                    // New Event
                    ev = new CalendarEvent
                    {
                        Title = request.Title.Trim(),
                        Type = request.Type,
                        Date = utcDate,
                        Time = request.Time,
                        Description = request.Description?.Trim(),
                        StudentId = studentId,
                        StudentName = studentName,
                        CreatedByProfessionalId = professional.Id,
                        TargetScope = request.TargetScope ?? "all"
                    };
                    await _calendarEventsRepository.CreateAsync(ev, cancellationToken);
                }
                else
                {
                    // Edit Event
                    ev.Title = request.Title.Trim();
                    ev.Type = request.Type;
                    ev.Date = utcDate;
                    ev.Time = request.Time;
                    ev.Description = request.Description?.Trim();
                    ev.StudentId = studentId;
                    ev.StudentName = studentName;
                    ev.TargetScope = request.TargetScope ?? "all";
                    ev.UpdatedAt = DateTime.UtcNow;
                    await _calendarEventsRepository.UpdateAsync(ev, cancellationToken);
                }

                await _unitOfWork.SaveChangesAsync(cancellationToken);

                // Notificaciones de calendario segmentadas
                await SendCalendarNotificationsAsync(professional, request, ev, studentId, studentName, cancellationToken);

                return Ok(ApiResponse<CalendarEventResponse>.SuccessResult(CalendarEventResponse.From(ev)));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<CalendarEventResponse>.ErrorResult(ErrorCode.InternalError, $"Error al procesar el evento: {ex.Message}"));
            }
        }

        private async Task SendCalendarNotificationsAsync(
            Professional professional,
            SaveCalendarEventRequest request,
            CalendarEvent ev,
            Guid? studentId,
            string? studentName,
            CancellationToken cancellationToken)
        {
            try
            {
                var professionalName = $"{professional.FirstName} {professional.LastName}";

                if (request.TargetScope == "single" && studentId.HasValue)
                {
                    var representatives = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(studentId.Value, cancellationToken);
                    var activeReps = representatives.Where(r => r.IsActive && r.Representative.IsActive).ToList();

                    if (!activeReps.Any()) return;

                    List<PersonRepresentative> targetReps;

                    // Regla de Negocio:
                    // 1. Si el tipo es "Tutoría" (Tutoría / Tutoría Personalizada): Solo al tutor principal (IsPrimary)
                    // 2. Si el tipo es "Clase" o "Tarea": A todos los tutores asignados
                    if (request.Type.Equals("Tutoría", StringComparison.OrdinalIgnoreCase))
                    {
                        var primary = activeReps.FirstOrDefault(r => r.IsPrimary) ?? activeReps.First();
                        targetReps = new List<PersonRepresentative> { primary };
                    }
                    else
                    {
                        targetReps = activeReps;
                    }

                    foreach (var rep in targetReps)
                    {
                        var tutorUserId = rep.Representative.UserId.ToString();

                        await _backgroundJobs.CreateAsync(
                            JobTypes.Push,
                            System.Text.Json.JsonSerializer.Serialize(new NotificationPayload
                            {
                                UserId = tutorUserId,
                                Title = $"Nuevo evento de calendario: {request.Type}",
                                Message = $"{professionalName} agendó una {request.Type} '{request.Title}' para {studentName}.",
                                ActionUrl = "calendar",
                                SendEmailFallback = false
                            }),
                            maxRetries: 3,
                            cancellationToken: cancellationToken);
                    }
                }
                else if (request.TargetScope == "all")
                {
                    // Notificación general (Clase / Tarea general) a todos los tutores de los alumnos asignados
                    var assignments = await _assignmentsRepository.GetPersonsByProfessionalIdAsync(professional.Id, cancellationToken);
                    var activePersonIds = assignments.Where(a => a.IsActive).Select(a => a.PersonId).Distinct().ToList();

                    var notifiedTutorIds = new HashSet<Guid>();

                    foreach (var personId in activePersonIds)
                    {
                        var representatives = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(personId, cancellationToken);
                        foreach (var rep in representatives.Where(r => r.IsActive && r.Representative.IsActive))
                        {
                            if (notifiedTutorIds.Add(rep.Representative.UserId))
                            {
                                var tutorUserId = rep.Representative.UserId.ToString();
                                await _backgroundJobs.CreateAsync(
                                    JobTypes.Push,
                                    System.Text.Json.JsonSerializer.Serialize(new NotificationPayload
                                    {
                                        UserId = tutorUserId,
                                        Title = $"Nuevo evento general: {request.Type}",
                                        Message = $"{professionalName} agendó una {request.Type} general '{request.Title}'.",
                                        ActionUrl = "calendar",
                                        SendEmailFallback = false
                                    }),
                                    maxRetries: 3,
                                    cancellationToken: cancellationToken);
                            }
                        }
                    }
                }
            }
            catch
            {
                // No romper la respuesta HTTP si la encolación de notificaciones falla
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

            return Ok(ApiResponse<object>.SuccessResult("Evento eliminado exitosamente."));
        }
    }
}
