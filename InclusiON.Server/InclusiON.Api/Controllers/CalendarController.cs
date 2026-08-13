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
        private readonly IHttpContextService _httpContextService;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly IUnitOfWork _unitOfWork;

        public CalendarController(
            ICalendarEventsRepository calendarEventsRepository,
            IProfessionalsRepository professionalsRepository,
            IFamilyRepository familyRepository,
            IPersonsRepository personsRepository,
            IHttpContextService httpContextService,
            IBackgroundJobRepository backgroundJobs,
            IUnitOfWork unitOfWork)
        {
            _calendarEventsRepository = calendarEventsRepository;
            _professionalsRepository = professionalsRepository;
            _familyRepository = familyRepository;
            _personsRepository = personsRepository;
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
            var userId = _httpContextService.GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var professional = await _professionalsRepository.GetByUserIdAsync(userId.Value, cancellationToken);
            if (professional == null) return Forbid();

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

            if (!DateTime.TryParse(request.Date, out var dateVal))
            {
                return BadRequest(ApiResponse<CalendarEventResponse>.ErrorResult(ErrorCode.ValidationFailed, "Fecha inválida. Formato esperado: YYYY-MM-DD"));
            }

            if (ev == null)
            {
                // New Event
                ev = new CalendarEvent
                {
                    Title = request.Title,
                    Type = request.Type,
                    Date = dateVal.Date,
                    Time = request.Time,
                    Description = request.Description,
                    StudentId = studentId,
                    StudentName = studentName,
                    CreatedByProfessionalId = professional.Id,
                    TargetScope = request.TargetScope
                };
                await _calendarEventsRepository.CreateAsync(ev, cancellationToken);
            }
            else
            {
                // Edit Event
                ev.Title = request.Title;
                ev.Type = request.Type;
                ev.Date = dateVal.Date;
                ev.Time = request.Time;
                ev.Description = request.Description;
                ev.StudentId = studentId;
                ev.StudentName = studentName;
                ev.TargetScope = request.TargetScope;
                ev.UpdatedAt = DateTime.UtcNow;
                await _calendarEventsRepository.UpdateAsync(ev, cancellationToken);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Send push notification to target student's tutors if single target scope
            if (request.TargetScope == "single" && studentId.HasValue)
            {
                try
                {
                    var representatives = await _familyRepository.GetPersonRepresentativesByPersonIdAsync(studentId.Value, cancellationToken);
                    foreach (var rep in representatives.Where(r => r.IsActive && r.Representative.IsActive))
                    {
                        var tutorUserId = rep.Representative.UserId.ToString();
                        var tutorName = $"{professional.FirstName} {professional.LastName}";
                        await _backgroundJobs.CreateAsync(
                            JobTypes.Push,
                            System.Text.Json.JsonSerializer.Serialize(new NotificationPayload
                            {
                                UserId = tutorUserId,
                                Title = "Nuevo evento de calendario",
                                Message = $"{tutorName} agendó un nuevo evento '{request.Title}' para {studentName}.",
                                ActionUrl = "calendar",
                                SendEmailFallback = false
                            }),
                            maxRetries: 3,
                            cancellationToken: cancellationToken);
                    }
                }
                catch (Exception)
                {
                    // Silent catch to avoid breaking the response if notification queuing fails
                }
            }

            return Ok(ApiResponse<CalendarEventResponse>.SuccessResult(CalendarEventResponse.From(ev)));
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

            return Ok(ApiResponse<object>.SuccessResult(null, "Evento eliminado exitosamente."));
        }
    }
}
