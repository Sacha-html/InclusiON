using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Api.Controllers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Requests.Calendar;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Calendar;

namespace InclusiON.Tests.Controllers;

public class CalendarAndNotificationsControllerTests
{
    private readonly ICalendarEventsRepository _calendarRepo = Substitute.For<ICalendarEventsRepository>();
    private readonly IProfessionalsRepository _profRepo = Substitute.For<IProfessionalsRepository>();
    private readonly IFamilyRepository _familyRepo = Substitute.For<IFamilyRepository>();
    private readonly IPersonsRepository _personsRepo = Substitute.For<IPersonsRepository>();
    private readonly IAssignmentsRepository _assignmentsRepo = Substitute.For<IAssignmentsRepository>();
    private readonly IHttpContextService _httpContext = Substitute.For<IHttpContextService>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();

    private CalendarController BuildCalendarController() => new(
        _calendarRepo,
        _profRepo,
        _familyRepo,
        _personsRepo,
        _assignmentsRepo,
        _httpContext,
        _notificationService,
        _dateTime,
        _uow,
        NullLogger<CalendarController>.Instance);

    [Fact]
    public async Task SaveEvent_SingleTutoria_SavesStudentIdAndNotifiesAllActiveTutors()
    {
        var profUserId = Guid.NewGuid();
        var profId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var primaryTutorUserId = Guid.NewGuid();
        var secondaryTutorUserId = Guid.NewGuid();

        _dateTime.Today.Returns(new DateOnly(2026, 11, 19));
        _dateTime.UtcNow.Returns(new DateTime(2026, 11, 19, 18, 0, 0, DateTimeKind.Utc));
        _httpContext.GetCurrentUserId().Returns(profUserId);
        _profRepo.GetByUserIdAsync(profUserId, Arg.Any<CancellationToken>())
            .Returns(new Professional { Id = profId, UserId = profUserId, FirstName = "María", LastName = "López", IsActive = true });
        _assignmentsRepo.GetAssignmentAsync(profId, studentId, Arg.Any<CancellationToken>())
            .Returns(new ProfessionalPerson { ProfessionalId = profId, PersonId = studentId, IsActive = true });
        _personsRepo.GetByIdAsync(studentId, Arg.Any<CancellationToken>())
            .Returns(new PersonWithDisability { Id = studentId, FirstName = "joan", LastName = "joan", IsActive = true });
        _familyRepo.GetPersonRepresentativesByPersonIdAsync(studentId, Arg.Any<CancellationToken>())
            .Returns(new List<PersonRepresentative>
            {
                new() { IsActive = true, IsPrimary = true, Representative = new FamilyRepresentative { UserId = primaryTutorUserId, IsActive = true, User = new User { IsActive = true } } },
                new() { IsActive = true, IsPrimary = false, Representative = new FamilyRepresentative { UserId = secondaryTutorUserId, IsActive = true, User = new User { IsActive = true } } },
            });

        List<NotificationMessage>? captured = null;
        _notificationService.NotifyManyAsync(Arg.Do<IEnumerable<NotificationMessage>>(m => captured = m.ToList()), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var request = new SaveCalendarEventRequest
        {
            Title = "Tutoría semanal",
            Type = "Tutoría",
            Date = "2026-11-20",
            Time = "15:00",
            TargetScope = "single",
            StudentId = studentId
        };

        var actionResult = await BuildCalendarController().SaveEvent(request, default);

        var ok = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        var apiResponse = ok.Value.Should().BeOfType<ApiResponse<CalendarEventResponse>>().Subject;
        apiResponse.Success.Should().BeTrue();
        apiResponse.Data!.StudentId.Should().Be(studentId);
        apiResponse.Data.StudentName.Should().Be("joan joan");
        apiResponse.Data.CreatedBy.Should().Be("María López");

        captured.Should().NotBeNull();
        captured!.Should().HaveCount(2);
        captured!.Select(n => n.UserId).Should().BeEquivalentTo([primaryTutorUserId, secondaryTutorUserId]);
        captured.Should().OnlyContain(n =>
            n.Type == NotificationTypes.Calendar &&
            n.EventCode == NotificationEventCodes.CalendarEventCreated &&
            n.ActionUrl == "/family/calendar");
    }

    [Fact]
    public async Task SaveEvent_SingleWithoutStudentId_ReturnsBadRequest()
    {
        var profUserId = Guid.NewGuid();
        _dateTime.Today.Returns(new DateOnly(2026, 11, 19));
        _httpContext.GetCurrentUserId().Returns(profUserId);
        _profRepo.GetByUserIdAsync(profUserId, Arg.Any<CancellationToken>())
            .Returns(new Professional { Id = Guid.NewGuid(), UserId = profUserId, IsActive = true });

        var actionResult = await BuildCalendarController().SaveEvent(new SaveCalendarEventRequest
        {
            Title = "Tutoría sin alumno",
            Type = "Tutoría",
            Date = "2026-11-20",
            Time = "10:00",
            TargetScope = "single",
            StudentId = null
        }, default);

        actionResult.Result.Should().BeOfType<BadRequestObjectResult>();
        await _calendarRepo.DidNotReceive().CreateAsync(Arg.Any<CalendarEvent>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteEvent_NotifiesAllActiveTutorsWithCancellation()
    {
        var profUserId = Guid.NewGuid();
        var profId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var tutorUserId = Guid.NewGuid();

        _httpContext.GetCurrentUserId().Returns(profUserId);
        _profRepo.GetByUserIdAsync(profUserId, Arg.Any<CancellationToken>())
            .Returns(new Professional { Id = profId, UserId = profUserId, FirstName = "María", LastName = "López", IsActive = true });
        _calendarRepo.GetByIdAsync(eventId, Arg.Any<CancellationToken>())
            .Returns(new CalendarEvent
            {
                Id = eventId,
                Title = "Tutoría cancelada",
                Type = "Tutoría",
                Date = new DateTime(2026, 11, 20, 0, 0, 0, DateTimeKind.Utc),
                Time = "15:00",
                TargetScope = "single",
                StudentId = studentId,
                StudentName = "joan joan",
                CreatedByProfessionalId = profId,
                IsActive = true
            });
        _familyRepo.GetPersonRepresentativesByPersonIdAsync(studentId, Arg.Any<CancellationToken>())
            .Returns(new List<PersonRepresentative>
            {
                new() { IsActive = true, Representative = new FamilyRepresentative { UserId = tutorUserId, IsActive = true, User = new User { IsActive = true } } }
            });

        List<NotificationMessage>? captured = null;
        _notificationService.NotifyManyAsync(Arg.Do<IEnumerable<NotificationMessage>>(m => captured = m.ToList()), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var result = await BuildCalendarController().DeleteEvent(eventId, default);

        result.Result.Should().BeOfType<OkObjectResult>();
        captured.Should().HaveCount(1);
        captured![0].UserId.Should().Be(tutorUserId);
        captured[0].EventCode.Should().Be(NotificationEventCodes.CalendarEventDeleted);
        captured[0].ActionUrl.Should().Be("/family/calendar");
    }

    [Fact]
    public async Task NotificationsController_GetMyNotifications_ReturnsOnlyCurrentUserItems()
    {
        var notifRepo = Substitute.For<INotificationsRepository>();
        var userId = Guid.NewGuid();
        _httpContext.GetCurrentUserId().Returns(userId);
        notifRepo.GetByUserIdAsync(userId, 1, 30, false, Arg.Any<CancellationToken>())
            .Returns(new List<Notification>
            {
                new() { Id = Guid.NewGuid(), UserId = userId, Type = NotificationTypes.Calendar, Title = "Tutoría", Message = "Mañana", ActionUrl = "/family/calendar" }
            });

        var controller = new NotificationsController(notifRepo, _httpContext, _dateTime);
        var result = await controller.GetMyNotifications(1, 30, false, default);

        var ok = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var body = ok.Value.Should().BeOfType<ApiResponse<List<InclusiON.DTOs.Responses.Notifications.NotificationResponse>>>().Subject;
        body.Data.Should().HaveCount(1);
        body.Data![0].Title.Should().Be("Tutoría");
    }
}
