using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Handlers;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.Domain.Models;

namespace InclusiON.Tests.Unit.Handlers.Professionals
{
    public class GetWeeklyProgressQueryHandlerTests
    {
        private readonly IAssignmentsRepository        _assignments = Substitute.For<IAssignmentsRepository>();
        private readonly IActivityAssignmentRepository _activities  = Substitute.For<IActivityAssignmentRepository>();
        private readonly IDateTimeProvider             _dateTime    = Substitute.For<IDateTimeProvider>();

        private static readonly Guid ProfId   = Guid.NewGuid();
        private static readonly Guid PersonId1 = Guid.NewGuid();
        private static readonly Guid PersonId2 = Guid.NewGuid();

        private static readonly DateTime Now = new(2026, 5, 25, 12, 0, 0, DateTimeKind.Utc);

        private GetWeeklyProgressQueryHandler BuildSut() =>
            new(_assignments, _activities, _dateTime);

        private static GetWeeklyProgressQuery Query() => new(ProfId);

        // ── Sin personas asignadas ───────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NoPeople_ReturnsZeroStats()
        {
            _dateTime.UtcNow.Returns(Now);
            _assignments.GetPersonsByProfessionalIdAsync(ProfId, Arg.Any<CancellationToken>())
                        .Returns(new List<ProfessionalPerson>());

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Success.Should().BeTrue();
            result.Data!.PersonCount.Should().Be(0);
            result.Data.TotalCompleted.Should().Be(0);
            result.Data.AvgSuccess.Should().Be(0m);
            result.Data.FrustrationAlerts.Should().Be(0);

            // No debe consultar actividades si no hay personas
            await _activities.DidNotReceive()
                .GetRecentCompletedResponsesByPersonIdsAsync(
                    Arg.Any<IEnumerable<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
        }

        // ── Con personas pero sin actividades en la semana ───────────────────

        [Fact]
        public async Task HandleAsync_PeopleButNoWeeklyActivity_ReturnZeroCompleted()
        {
            _dateTime.UtcNow.Returns(Now);
            _assignments.GetPersonsByProfessionalIdAsync(ProfId, Arg.Any<CancellationToken>())
                        .Returns(new List<ProfessionalPerson>
                        {
                            new() { PersonId = PersonId1 },
                        });

            // Actividad fuera del período (hace 8 días)
            var old = new ActivityResponse
            {
                CompletedAt       = Now.AddDays(-8),
                SuccessPercentage = 80m,
                FrustrationLevel  = 1,
            };

            _activities.GetRecentCompletedResponsesByPersonIdsAsync(
                Arg.Any<IEnumerable<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, List<ActivityResponse>>
            {
                { PersonId1, new List<ActivityResponse> { old } },
            });

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Success.Should().BeTrue();
            result.Data!.PersonCount.Should().Be(1);
            result.Data.TotalCompleted.Should().Be(0);
            result.Data.AvgSuccess.Should().Be(0m);
            result.Data.FrustrationAlerts.Should().Be(0);
        }

        // ── Happy path: 2 personas, varias actividades ────────────────────────

        [Fact]
        public async Task HandleAsync_WithWeeklyActivity_ComputesCorrectStats()
        {
            _dateTime.UtcNow.Returns(Now);
            _assignments.GetPersonsByProfessionalIdAsync(ProfId, Arg.Any<CancellationToken>())
                        .Returns(new List<ProfessionalPerson>
                        {
                            new() { PersonId = PersonId1 },
                            new() { PersonId = PersonId2 },
                        });

            var responses = new Dictionary<Guid, List<ActivityResponse>>
            {
                {
                    PersonId1, new List<ActivityResponse>
                    {
                        new() { CompletedAt = Now.AddDays(-1), SuccessPercentage = 80m, FrustrationLevel = 2 },
                        new() { CompletedAt = Now.AddDays(-2), SuccessPercentage = 60m, FrustrationLevel = 5 }, // alerta
                    }
                },
                {
                    PersonId2, new List<ActivityResponse>
                    {
                        new() { CompletedAt = Now.AddDays(-3), SuccessPercentage = 100m, FrustrationLevel = 4 }, // alerta
                        new() { CompletedAt = Now.AddDays(-4), SuccessPercentage = null,  FrustrationLevel = 1 }, // sin %
                    }
                },
            };

            _activities.GetRecentCompletedResponsesByPersonIdsAsync(
                Arg.Any<IEnumerable<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(responses);

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Success.Should().BeTrue();
            result.Data!.PersonCount.Should().Be(2);
            result.Data.TotalCompleted.Should().Be(4);
            // avg de 80, 60, 100 (null excluido) → 240/3 = 80, redondeado
            result.Data.AvgSuccess.Should().Be(80m);
            // FrustrationLevel >= 4: niveles 5 y 4 → 2 alertas
            result.Data.FrustrationAlerts.Should().Be(2);
        }

        // ── Actividades sin SuccessPercentage ────────────────────────────────

        [Fact]
        public async Task HandleAsync_AllNullSuccessPercentage_AvgSuccessIsZero()
        {
            _dateTime.UtcNow.Returns(Now);
            _assignments.GetPersonsByProfessionalIdAsync(ProfId, Arg.Any<CancellationToken>())
                        .Returns(new List<ProfessionalPerson>
                        {
                            new() { PersonId = PersonId1 },
                        });

            _activities.GetRecentCompletedResponsesByPersonIdsAsync(
                Arg.Any<IEnumerable<Guid>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new Dictionary<Guid, List<ActivityResponse>>
            {
                {
                    PersonId1, new List<ActivityResponse>
                    {
                        new() { CompletedAt = Now.AddDays(-1), SuccessPercentage = null, FrustrationLevel = 1 },
                        new() { CompletedAt = Now.AddDays(-2), SuccessPercentage = null, FrustrationLevel = 2 },
                    }
                },
            });

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalCompleted.Should().Be(2);
            result.Data.AvgSuccess.Should().Be(0m);
        }

        // ── Período correcto ─────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_PeriodDates_AreCorrect()
        {
            _dateTime.UtcNow.Returns(Now);
            _assignments.GetPersonsByProfessionalIdAsync(ProfId, Arg.Any<CancellationToken>())
                        .Returns(new List<ProfessionalPerson>());

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Data!.PeriodEnd.Should().Be(Now);
            result.Data.PeriodStart.Should().Be(Now.AddDays(-7));
        }
    }
}
