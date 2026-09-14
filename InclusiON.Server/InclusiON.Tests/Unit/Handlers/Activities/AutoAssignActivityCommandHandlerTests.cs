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
using InclusiON.DTOs.Common;
using ActivityAssignment = InclusiON.Domain.Models.ActivityAssignment;

namespace InclusiON.Tests.Unit.Handlers.Activities;

public class AutoAssignActivityCommandHandlerTests
{
    private readonly IActivityAssignmentRepository _assignments = Substitute.For<IActivityAssignmentRepository>();
    private readonly IRoadmapRepository _roadmaps = Substitute.For<IRoadmapRepository>();
    private readonly IActivitiesRepository _activities = Substitute.For<IActivitiesRepository>();
    private readonly IPersonsRepository _persons = Substitute.For<IPersonsRepository>();
    private readonly IProfessionalsRepository _professionals = Substitute.For<IProfessionalsRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
    private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
    private static readonly Guid PersonId = Guid.NewGuid();
    private const int ActivityId = 7;

    private AutoAssignActivityCommandHandler Sut() => new(
        _assignments, _roadmaps, _activities, _persons, _professionals,
        _unitOfWork, _dateTime, _encryption);

    private static PersonRoadmapActivity Entry(bool unlocked = true) => new()
    {
        ActivityId = ActivityId,
        IsUnlocked = unlocked,
        Activity = new Activity { Id = ActivityId, Title = "AAC activity", IsActive = true }
    };

    private static Activity CatalogActivity() => new()
    {
        Id = ActivityId, Title = "AAC activity", IsActive = true, IsStandardActivity = true
    };

    [Fact]
    public async Task ActivityNotInPersonRoadmap_IsForbiddenAndDoesNotCreate()
    {
        _roadmaps.GetByPersonAndActivityAsync(PersonId, ActivityId, Arg.Any<CancellationToken>())
            .Returns((PersonRoadmapActivity?)null);

        var result = await Sut().HandleAsync(new(ActivityId, PersonId), default);

        result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        await _assignments.DidNotReceive().CreateAsync(Arg.Any<ActivityAssignment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LockedRoadmapActivity_IsForbidden()
    {
        _roadmaps.GetByPersonAndActivityAsync(PersonId, ActivityId, Arg.Any<CancellationToken>())
            .Returns(Entry(unlocked: false));

        var result = await Sut().HandleAsync(new(ActivityId, PersonId), default);

        result.ErrorCode.Should().Be(ErrorCode.Forbidden);
    }

    [Fact]
    public async Task ExistingActiveAssignment_IsReturnedWithoutDuplicate()
    {
        _roadmaps.GetByPersonAndActivityAsync(PersonId, ActivityId, Arg.Any<CancellationToken>()).Returns(Entry());
        var existing = new ActivityAssignment { Id = 12, ActivityId = ActivityId, PersonId = PersonId, StatusId = AssignmentStatuses.Pendiente };
        _activities.GetByIdAsync(ActivityId, Arg.Any<CancellationToken>()).Returns(CatalogActivity());
        _assignments.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(new List<ActivityAssignment> { existing });
        _encryption.Encrypt(Arg.Any<string>()).Returns("encrypted-assignment");

        var result = await Sut().HandleAsync(new(ActivityId, PersonId), default);

        result.Success.Should().BeTrue();
        result.Data!.EncryptedId.Should().Be("encrypted-assignment");
        await _assignments.DidNotReceive().CreateAsync(Arg.Any<ActivityAssignment>(), Arg.Any<CancellationToken>());
    }
}
