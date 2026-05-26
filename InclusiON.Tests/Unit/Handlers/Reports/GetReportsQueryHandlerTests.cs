using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Handlers;
using InclusiON.Application.UseCases.Reports.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Reports
{
    public class GetReportsQueryHandlerTests
    {
        private readonly IReportsRepository _repo = Substitute.For<IReportsRepository>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();

        private static Report AReport() => new()
        {
            Id = 1, IsActive = true, ReportDate = DateTime.UtcNow,
        };

        // ── GetReports (paged) ───────────────────────────────────────────────

        [Fact]
        public async Task GetReports_ReturnsMappedPagedResponse()
        {
            _repo.GetPagedAsync(
                Arg.Any<int>(), Arg.Any<int>(), Arg.Any<string?>(),
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(),
                Arg.Any<bool?>(), Arg.Any<string?>(),
                Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
                Arg.Any<SortField?>(), Arg.Any<string>(), Arg.Any<List<int>?>(),
                Arg.Any<List<string>?>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<Report>
            {
                Data = new List<Report> { AReport() },
                TotalRecords = 1, TotalPages = 1, CurrentPage = 1, PageSize = 10,
            });

            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:test");
            var handler = new GetReportsQueryHandler(_repo, _encryption);
            var result = await handler.HandleAsync(
                new GetReportsQuery(1, 10, null, null, null, null, null, null, null, null, null, "asc", null), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(1);
            result.Data.Data.Should().HaveCount(1);
        }

        // ── GetFamilyReports (paged) ─────────────────────────────────────────

        [Fact]
        public async Task GetFamilyReports_ReturnsMappedPagedResponse()
        {
            var familyId = Guid.NewGuid();
            _repo.GetFamilyPagedAsync(
                familyId,
                Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<string?>(), Arg.Any<DateTime?>(), Arg.Any<DateTime?>(),
                Arg.Any<SortField?>(), Arg.Any<string>(),
                Arg.Any<CancellationToken>())
            .Returns(new PagedResponse<Report>
            {
                Data = new List<Report> { AReport() },
                TotalRecords = 1, TotalPages = 1, CurrentPage = 1, PageSize = 10,
            });

            _encryption.Encrypt(Arg.Any<string>()).Returns("ENC:test");
            var handler = new GetFamilyReportsQueryHandler(_repo, _encryption);
            var result = await handler.HandleAsync(
                new GetFamilyReportsQuery(familyId, 1, 10, null, null, null, null, "asc"), default);

            result.Success.Should().BeTrue();
            result.Data!.TotalRecords.Should().Be(1);
        }
    }
}
