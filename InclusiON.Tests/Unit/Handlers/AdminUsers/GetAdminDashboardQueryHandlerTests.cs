using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.UseCases.AdminUsers.Handlers;
using InclusiON.Application.UseCases.AdminUsers.Queries;

namespace InclusiON.Tests.Unit.Handlers.AdminUsers
{
    public class GetAdminDashboardQueryHandlerTests
    {
        private readonly IRawDbExecutor _db = Substitute.For<IRawDbExecutor>();

        private GetAdminDashboardQueryHandler BuildSut() => new(_db);

        /// <summary>
        /// Makes every ExecuteScalarAsync call return the given value.
        /// </summary>
        private void SetupAllScalars(int value)
        {
            _db.ExecuteScalarAsync<int>(
                    Arg.Any<string>(),
                    Arg.Any<Action<System.Data.IDbCommand>?>(),
                    Arg.Any<CancellationToken>())
               .Returns(value);
        }

        // ── GlobalAdmin ───────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_GlobalAdmin_AllKpisPopulatedAndSuccessful()
        {
            // All queries return 5 — verifies every KPI is wired up.
            SetupAllScalars(5);

            var result = await BuildSut().HandleAsync(
                new GetAdminDashboardQuery(IsGlobalAdmin: true, InstitutionIds: []), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalProfessionals.Should().Be(5);
            result.Data.PendingValidations.Should().Be(5);
            result.Data.TotalFamilies.Should().Be(5);
            result.Data.TotalPersons.Should().Be(5);
            result.Data.TotalInstitutions.Should().Be(5);   // GlobalAdmin — must be set
            result.Data.ActiveAssignments.Should().Be(5);
            result.Data.ReportsPendingApproval.Should().Be(5);
            result.Data.ReportsApprovedThisMonth.Should().Be(5);
        }

        [Fact]
        public async Task HandleAsync_GlobalAdmin_InstitutionsQueryIsExecuted()
        {
            SetupAllScalars(0);

            // Only the Institutions query returns a non-zero value.
            _db.ExecuteScalarAsync<int>(
                    Arg.Is<string>(s => s.Contains("\"EducationalInstitutions\"")),
                    Arg.Any<Action<System.Data.IDbCommand>?>(),
                    Arg.Any<CancellationToken>())
               .Returns(8);

            var result = await BuildSut().HandleAsync(
                new GetAdminDashboardQuery(IsGlobalAdmin: true, InstitutionIds: []), default);

            result.Data!.TotalInstitutions.Should().Be(8);
        }

        // ── AdminInstitucional ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AdminInstitucional_TotalInstitutionsIsNull()
        {
            SetupAllScalars(3);

            var result = await BuildSut().HandleAsync(
                new GetAdminDashboardQuery(IsGlobalAdmin: false, InstitutionIds: [1, 2]), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalInstitutions.Should().BeNull();
        }

        [Fact]
        public async Task HandleAsync_AdminInstitucional_ScopedQueriesContainInstFilter()
        {
            SetupAllScalars(0);

            var capturedSqls = new List<string>();
            _db.ExecuteScalarAsync<int>(
                    Arg.Do<string>(sql => capturedSqls.Add(sql)),
                    Arg.Any<Action<System.Data.IDbCommand>?>(),
                    Arg.Any<CancellationToken>())
               .Returns(0);

            await BuildSut().HandleAsync(
                new GetAdminDashboardQuery(IsGlobalAdmin: false, InstitutionIds: [42]), default);

            // At least the professional query should filter by institution
            capturedSqls.Should().Contain(s => s.Contains("ProfessionalInstitutions"));
        }

        // ── Edge cases ────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AllZeros_ReturnsSuccessWithZeroKpis()
        {
            SetupAllScalars(0);

            var result = await BuildSut().HandleAsync(
                new GetAdminDashboardQuery(IsGlobalAdmin: true, InstitutionIds: []), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalProfessionals.Should().Be(0);
            result.Data.PendingValidations.Should().Be(0);
            result.Data.TotalFamilies.Should().Be(0);
            result.Data.TotalPersons.Should().Be(0);
            result.Data.TotalInstitutions.Should().Be(0);
            result.Data.ActiveAssignments.Should().Be(0);
            result.Data.ReportsPendingApproval.Should().Be(0);
            result.Data.ReportsApprovedThisMonth.Should().Be(0);
        }
    }
}
