using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Activities.Handlers;
using InclusiON.Application.UseCases.Activities.Queries;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using Activity = InclusiON.Domain.Models.Activity;
using ActivityResponse = InclusiON.Domain.Models.ActivityResponse;

namespace InclusiON.Tests.Unit.Handlers.Activities
{
    public class ActivityQueryHandlersTests
    {
        private readonly IActivitiesRepository _activitiesRepo = Substitute.For<IActivitiesRepository>();
        private readonly IActivityAssignmentRepository _assignRepo = Substitute.For<IActivityAssignmentRepository>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();
        private readonly IFamilyRepository _familyRepository = Substitute.For<IFamilyRepository>();

        private static readonly Guid ProfId = Guid.NewGuid();
        private static readonly Guid PersonId = Guid.NewGuid();

        public ActivityQueryHandlersTests()
        {
            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:test");
            _familyRepository.GetPersonRepresentativesByPersonIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
                .Returns(new List<PersonRepresentative>());
        }

        private static Activity AnActivity(int id = 1, bool isStandard = false) => new()
        {
            Id = id,
            Title = "Actividad Test",
            ProfessionalId = ProfId,
            IsActive = true,
            IsStandardActivity = isStandard,
            CategoryId = 1,
            CreatedAt = DateTime.UtcNow,
        };

        // ── GetActivities (paged) ─────────────────────────────────────────

        [Fact]
        public async Task GetActivities_ReturnsPagedResult()
        {
            _activitiesRepo.GetPagedAsync(
                    ProfId, null, null, null, null, null, null,
                    Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns((new List<Activity> { AnActivity(1), AnActivity(2) }, 2));

            var handler = new GetActivitiesQueryHandler(_activitiesRepo, _encryption);
            var query = new GetActivitiesQuery(ProfId, null, null, null, null, null, null, 1, 10);
            var result = await handler.HandleAsync(query, default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(2);
            result.Data.Data.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetActivities_EmptyResult_ReturnsZeroRecords()
        {
            _activitiesRepo.GetPagedAsync(
                    Arg.Any<Guid>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>(),
                    Arg.Any<int?>(), Arg.Any<bool?>(), Arg.Any<bool?>(),
                    Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns((new List<Activity>(), 0));

            var query = new GetActivitiesQuery(ProfId, null, null, null, null, null, null, 1, 10);
            var result = await new GetActivitiesQueryHandler(_activitiesRepo, _encryption).HandleAsync(query, default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(0);
            result.Data.Data.Should().BeEmpty();
        }

        // ── GetActivityById ──────────────────────────────────────────────

        [Fact]
        public async Task GetActivityById_NotFound_ReturnsNotFound()
        {
            _activitiesRepo.GetByIdAsync(99, Arg.Any<CancellationToken>())
                .Returns((Activity?)null);

            var result = await new GetActivityByIdQueryHandler(_activitiesRepo, _encryption)
                .HandleAsync(new GetActivityByIdQuery(99, ProfId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task GetActivityById_BelongsToOtherProfessional_ReturnsForbidden()
        {
            var otherProfId = Guid.NewGuid();
            var activity = AnActivity(1, isStandard: false);
            activity.ProfessionalId = otherProfId;

            _activitiesRepo.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns(activity);

            var result = await new GetActivityByIdQueryHandler(_activitiesRepo, _encryption)
                .HandleAsync(new GetActivityByIdQuery(1, ProfId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        [Fact]
        public async Task GetActivityById_StandardActivity_ReturnsSuccess()
        {
            var activity = AnActivity(1, isStandard: true);

            _activitiesRepo.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns(activity);

            var result = await new GetActivityByIdQueryHandler(_activitiesRepo, _encryption)
                .HandleAsync(new GetActivityByIdQuery(1, Guid.NewGuid()), default);

            result.Success.Should().BeTrue();
            result.Data!.Title.Should().Be("Actividad Test");
        }

        [Fact]
        public async Task GetActivityById_OwnedActivity_ReturnsSuccess()
        {
            var activity = AnActivity(1, isStandard: false);

            _activitiesRepo.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns(activity);

            var result = await new GetActivityByIdQueryHandler(_activitiesRepo, _encryption)
                .HandleAsync(new GetActivityByIdQuery(1, ProfId), default);

            result.Success.Should().BeTrue();
        }

        // ── GetPersonActivityAssignments ─────────────────────────────────

        [Fact]
        public async Task GetPersonActivityAssignments_ReturnsMappedList()
        {
            _assignRepo.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                .Returns(new List<ActivityAssignment>
                {
                    new()
                    {
                        Id = 1, PersonId = PersonId, ActivityId = 1,
                        StatusId = AssignmentStatuses.Pendiente, AssignedAt = DateTime.UtcNow,
                        Responses = new List<ActivityResponse>()
                    }
                });

            var handler = new GetPersonActivityAssignmentsQueryHandler(_assignRepo, _encryption, _familyRepository);
            // Pass PersonId as RequesterId so the student sees their own assignments
            var result = await handler.HandleAsync(
                new GetPersonActivityAssignmentsQuery(PersonId, PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data![0].PersonId.Should().Be(PersonId);
        }

        [Fact]
        public async Task GetPersonActivityAssignments_NoAssignments_ReturnsEmptyList()
        {
            _assignRepo.GetByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                .Returns(new List<ActivityAssignment>());

            var result = await new GetPersonActivityAssignmentsQueryHandler(_assignRepo, _encryption, _familyRepository)
                .HandleAsync(new GetPersonActivityAssignmentsQuery(PersonId, PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        // ── GetAssignmentById ────────────────────────────────────────────

        [Fact]
        public async Task GetAssignmentById_NotFound_ReturnsNotFound()
        {
            _assignRepo.GetByIdAsync(99, Arg.Any<CancellationToken>())
                .Returns((ActivityAssignment?)null);

            var result = await new GetAssignmentByIdQueryHandler(_assignRepo, _encryption)
                .HandleAsync(new GetAssignmentByIdQuery(99, PersonId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }
    }
}
