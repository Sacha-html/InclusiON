using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Family.Handlers;
using InclusiON.Application.UseCases.Family.Queries;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Family
{
    public class GetFamilyDashboardQueryHandlerTests
    {
        private readonly IFamilyRepository             _family      = Substitute.For<IFamilyRepository>();
        private readonly IActivityAssignmentRepository _assignments = Substitute.For<IActivityAssignmentRepository>();
        private readonly IReportsRepository            _reports     = Substitute.For<IReportsRepository>();
        private readonly IMessagesRepository           _messages    = Substitute.For<IMessagesRepository>();

        private static readonly Guid FamilyUserId = Guid.NewGuid();
        private static readonly Guid PersonId     = Guid.NewGuid();

        private GetFamilyDashboardQueryHandler BuildSut() =>
            new(_family, _assignments, _reports, _messages);

        private static PersonWithDisability APerson(Guid id) => new()
        {
            Id        = id,
            FirstName = "María",
            LastName  = "García",
            IsActive  = true,
            AvatarColor = "#A3C4BC"
        };

        private static ActivityResponse ACompletedResponse(int id, int assignmentId) => new()
        {
            Id               = id,
            AssignmentId     = assignmentId,
            CompletedAt      = DateTime.UtcNow.AddDays(-1),
            Result           = ActivityResponseResult.Exito,
            SuccessPercentage = 90m,
            Assignment       = new ActivityAssignment
            {
                Id         = assignmentId,
                Activity   = new Activity { Id = 1, Title = "Actividad Demo" }
            }
        };

        [Fact]
        public async Task HandleAsync_NoLinkedPersons_ReturnsEmptyPersonsList()
        {
            _family.GetLinkedPersonsAsync(FamilyUserId, Arg.Any<CancellationToken>())
                   .Returns(new List<PersonWithDisability>());
            _messages.GetUnreadCountAsync(FamilyUserId, Arg.Any<CancellationToken>())
                     .Returns(3);

            var result = await BuildSut().HandleAsync(new GetFamilyDashboardQuery(FamilyUserId), default);

            result.Success.Should().BeTrue();
            result.Data!.Persons.Should().BeEmpty();
            result.Data.UnreadMessages.Should().Be(3);
        }

        [Fact]
        public async Task HandleAsync_WithLinkedPerson_ReturnsPersonSummary()
        {
            var person = APerson(PersonId);
            _family.GetLinkedPersonsAsync(FamilyUserId, Arg.Any<CancellationToken>())
                   .Returns(new List<PersonWithDisability> { person });
            _messages.GetUnreadCountAsync(FamilyUserId, Arg.Any<CancellationToken>())
                     .Returns(0);
            _assignments.GetRecentCompletedResponsesByPersonIdsAsync(
                    Arg.Any<IEnumerable<Guid>>(), 3, Arg.Any<CancellationToken>())
                .Returns(new Dictionary<Guid, List<ActivityResponse>>
                {
                    [PersonId] = []
                });
            _reports.GetApprovedReportsSummaryByPersonIdsAsync(
                    Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(new Dictionary<Guid, (int Count, Report? Latest)>
                {
                    [PersonId] = (0, null)
                });

            var result = await BuildSut().HandleAsync(new GetFamilyDashboardQuery(FamilyUserId), default);

            result.Success.Should().BeTrue();
            result.Data!.Persons.Should().HaveCount(1);
            result.Data.Persons[0].PersonId.Should().Be(PersonId);
            result.Data.Persons[0].FullName.Should().Be("María García");
            result.Data.Persons[0].AvatarColor.Should().Be("#A3C4BC");
            result.Data.Persons[0].RecentActivities.Should().BeEmpty();
            result.Data.Persons[0].ApprovedReportsCount.Should().Be(0);
        }

        [Fact]
        public async Task HandleAsync_WithRecentActivities_MapsCorrectly()
        {
            _family.GetLinkedPersonsAsync(FamilyUserId, Arg.Any<CancellationToken>())
                   .Returns(new List<PersonWithDisability> { APerson(PersonId) });
            _messages.GetUnreadCountAsync(FamilyUserId, Arg.Any<CancellationToken>())
                     .Returns(0);
            _assignments.GetRecentCompletedResponsesByPersonIdsAsync(
                    Arg.Any<IEnumerable<Guid>>(), 3, Arg.Any<CancellationToken>())
                .Returns(new Dictionary<Guid, List<ActivityResponse>>
                {
                    [PersonId] = [ACompletedResponse(1, 10), ACompletedResponse(2, 11)]
                });
            _reports.GetApprovedReportsSummaryByPersonIdsAsync(
                    Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(new Dictionary<Guid, (int Count, Report? Latest)>
                {
                    [PersonId] = (0, null)
                });

            var result = await BuildSut().HandleAsync(new GetFamilyDashboardQuery(FamilyUserId), default);

            var activities = result.Data!.Persons[0].RecentActivities;
            activities.Should().HaveCount(2);
            activities[0].ActivityTitle.Should().Be("Actividad Demo");
            activities[0].Result.Should().Be(ActivityResponseResult.Exito.ToString());
            activities[0].SuccessPercentage.Should().Be(90m);
        }

        [Fact]
        public async Task HandleAsync_WithApprovedReports_MapsCountAndLatest()
        {
            var latestReport = new Report
            {
                Id         = 5,
                Title      = "Reporte Junio",
                ReportDate = new DateTime(2026, 6, 1)
            };

            _family.GetLinkedPersonsAsync(FamilyUserId, Arg.Any<CancellationToken>())
                   .Returns(new List<PersonWithDisability> { APerson(PersonId) });
            _messages.GetUnreadCountAsync(FamilyUserId, Arg.Any<CancellationToken>())
                     .Returns(0);
            _assignments.GetRecentCompletedResponsesByPersonIdsAsync(
                    Arg.Any<IEnumerable<Guid>>(), 3, Arg.Any<CancellationToken>())
                .Returns(new Dictionary<Guid, List<ActivityResponse>>
                {
                    [PersonId] = []
                });
            _reports.GetApprovedReportsSummaryByPersonIdsAsync(
                    Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())
                .Returns(new Dictionary<Guid, (int Count, Report? Latest)>
                {
                    [PersonId] = (3, latestReport)
                });

            var result = await BuildSut().HandleAsync(new GetFamilyDashboardQuery(FamilyUserId), default);

            var summary = result.Data!.Persons[0];
            summary.ApprovedReportsCount.Should().Be(3);
            summary.LatestReportTitle.Should().Be("Reporte Junio");
            summary.LatestReportDate.Should().Be(new DateTime(2026, 6, 1));
        }

        [Fact]
        public async Task HandleAsync_UnreadMessages_PassedThroughCorrectly()
        {
            _family.GetLinkedPersonsAsync(FamilyUserId, Arg.Any<CancellationToken>())
                   .Returns(new List<PersonWithDisability>());
            _messages.GetUnreadCountAsync(FamilyUserId, Arg.Any<CancellationToken>())
                     .Returns(7);

            var result = await BuildSut().HandleAsync(new GetFamilyDashboardQuery(FamilyUserId), default);

            result.Data!.UnreadMessages.Should().Be(7);
        }
    }
}
