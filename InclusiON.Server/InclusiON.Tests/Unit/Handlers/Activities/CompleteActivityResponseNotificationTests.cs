using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Commands;
using InclusiON.Application.UseCases.Activities.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Activities;

public class CompleteActivityResponseNotificationTests
{
    private readonly IActivityAssignmentRepository _repo = Substitute.For<IActivityAssignmentRepository>();
    private readonly IRoadmapRepository _roadmapRepo = Substitute.For<IRoadmapRepository>();
    private readonly IProfessionalsRepository _profRepo = Substitute.For<IProfessionalsRepository>();
    private readonly IBackgroundJobRepository _bgJobs = Substitute.For<IBackgroundJobRepository>();
    private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
    private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
    private readonly IRealTimeNotifier _realTime = Substitute.For<IRealTimeNotifier>();
    private readonly IFamilyRepository _familyRepo = Substitute.For<IFamilyRepository>();
    private readonly INotificationService _notificationService = Substitute.For<INotificationService>();

    private CompleteActivityResponseCommandHandler BuildSut() => new(
        _repo,
        _roadmapRepo,
        _profRepo,
        _bgJobs,
        _uow,
        _dateTime,
        _encryption,
        _realTime,
        _familyRepo,
        _notificationService);

    private (ActivityAssignment Assignment, ActivityResponse Response, Guid ProfUserId, Guid TutorUserId) SetupScenario(int attemptCount)
    {
        var personId = Guid.NewGuid();
        var profId = Guid.NewGuid();
        var profUserId = Guid.NewGuid();
        var tutorUserId = Guid.NewGuid();

        var assignment = new ActivityAssignment
        {
            Id = 10,
            ActivityId = 5,
            PersonId = personId,
            AssignedByProfessionalId = profId,
            StatusId = AssignmentStatuses.EnProgreso,
            Person = new PersonWithDisability { Id = personId, FirstName = "joan", LastName = "joan" },
            Activity = new Activity { Id = 5, Title = "Rompecabezas" }
        };

        var response = new ActivityResponse
        {
            Id = 20,
            AssignmentId = 10,
            AttemptCount = attemptCount
        };

        _dateTime.UtcNow.Returns(new DateTime(2026, 11, 19, 18, 0, 0, DateTimeKind.Utc));
        _encryption.Encrypt(Arg.Any<string>()).Returns(ci => $"ENC_{ci.Arg<string>()}");
        _repo.GetByIdAsync(10, Arg.Any<CancellationToken>()).Returns(assignment);
        _repo.GetResponseByIdAsync(20, Arg.Any<CancellationToken>()).Returns(response);
        _profRepo.GetByIdAsync(profId, Arg.Any<CancellationToken>())
            .Returns(new Professional { Id = profId, UserId = profUserId, FirstName = "Ana", LastName = "Pro" });
        _familyRepo.GetPersonRepresentativesByPersonIdAsync(personId, Arg.Any<CancellationToken>())
            .Returns(new List<PersonRepresentative>
            {
                new() { IsActive = true, Representative = new FamilyRepresentative { UserId = tutorUserId, IsActive = true } }
            });

        return (assignment, response, profUserId, tutorUserId);
    }

    [Fact]
    public async Task FailedAttemptUnderFour_DoesNotSendAnyNotification_AvoidsNoise()
    {
        var (assignment, _, _, _) = SetupScenario(attemptCount: 2);

        var cmd = new CompleteActivityResponseCommand(
            AssignmentId: 10,
            ResponseId: 20,
            PersonId: assignment.PersonId,
            TimeSpentSeconds: 30,
            SuccessPercentage: 40m,
            RequiredSupport: false,
            FrustrationLevel: null,
            ResponsePattern: null,
            Observations: null);

        var res = await BuildSut().HandleAsync(cmd, default);

        res.Success.Should().BeTrue();
        await _notificationService.DidNotReceive().NotifyManyAsync(Arg.Any<IEnumerable<NotificationMessage>>(), Arg.Any<CancellationToken>());
        await _realTime.DidNotReceiveWithAnyArgs().NotifyUserAsync(default!, default!, default!, default, default);
        await _bgJobs.DidNotReceive().CreateAsync(JobTypes.Push, Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task FourthFailedAttempt_CB07_SendsSingleAlertToProfAndTutorWithValidRoutes()
    {
        var (assignment, _, profUserId, tutorUserId) = SetupScenario(attemptCount: 4);

        List<NotificationMessage>? captured = null;
        _notificationService.NotifyManyAsync(Arg.Do<IEnumerable<NotificationMessage>>(m => captured = m.ToList()), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var cmd = new CompleteActivityResponseCommand(
            AssignmentId: 10,
            ResponseId: 20,
            PersonId: assignment.PersonId,
            TimeSpentSeconds: 45,
            SuccessPercentage: 20m,
            RequiredSupport: true,
            FrustrationLevel: 4,
            ResponsePattern: null,
            Observations: null);

        var res = await BuildSut().HandleAsync(cmd, default);

        res.Success.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Should().HaveCount(2);

        var profNotif = captured!.Single(n => n.UserId == profUserId);
        profNotif.Type.Should().Be(NotificationTypes.Alert);
        profNotif.ActionUrl.Should().Be($"/pro/evaluations?personId=ENC_{assignment.PersonId}&activityId=5&alert=attempts");

        var tutorNotif = captured!.Single(n => n.UserId == tutorUserId);
        tutorNotif.Type.Should().Be(NotificationTypes.Alert);
        tutorNotif.ActionUrl.Should().Be("/family");

        // Cero duplicados vía SignalR directo o JobTypes.Push
        await _realTime.DidNotReceiveWithAnyArgs().NotifyUserAsync(default!, default!, default!, default, default);
        await _bgJobs.DidNotReceive().CreateAsync(JobTypes.Push, Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SuccessfulCompletion_SendsSingleNotificationToProfAndTutorWithEncryptedPersonId()
    {
        var (assignment, _, profUserId, tutorUserId) = SetupScenario(attemptCount: 1);

        List<NotificationMessage>? captured = null;
        _notificationService.NotifyManyAsync(Arg.Do<IEnumerable<NotificationMessage>>(m => captured = m.ToList()), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var cmd = new CompleteActivityResponseCommand(
            AssignmentId: 10,
            ResponseId: 20,
            PersonId: assignment.PersonId,
            TimeSpentSeconds: 25,
            SuccessPercentage: 90m,
            RequiredSupport: false,
            FrustrationLevel: null,
            ResponsePattern: null,
            Observations: null);

        var res = await BuildSut().HandleAsync(cmd, default);

        res.Success.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Should().HaveCount(2);
        captured!.Single(n => n.UserId == profUserId).ActionUrl.Should().Be($"/pro/evaluations?personId=ENC_{assignment.PersonId}");
        captured!.Single(n => n.UserId == tutorUserId).ActionUrl.Should().Be("/family");
    }
}
