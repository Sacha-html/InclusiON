using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Handlers;
using InclusiON.Application.UseCases.Reports.Queries;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Reports
{
    public class ExportReportPdfQueryHandlerTests
    {
        private readonly IReportsRepository _reportsRepo = Substitute.For<IReportsRepository>();
        private readonly IReportPdfService  _pdfService  = Substitute.For<IReportPdfService>();

        private ExportReportPdfQueryHandler BuildSut() =>
            new(_reportsRepo, _pdfService);

        private static ExportReportPdfQuery Query() => new(ReportId: 1);

        private static Report AReport() => new()
        {
            Id             = 1,
            ProfessionalId = Guid.NewGuid(),
            Status         = ReportStatus.Approved,
            IsActive       = true,
            IsReadByFamily = true,
            ReportDate     = DateTime.UtcNow,
            CreatedAt      = DateTime.UtcNow,
        };

        // ── Reporte no encontrado ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ReportNotFound_ReturnsReportNotFound()
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Report?)null);

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ReportNotFound);
            _pdfService.DidNotReceive().Generate(Arg.Any<Report>());
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ReportFound_GeneratesPdfAndReturnsBytes()
        {
            var report       = AReport();
            var expectedBytes = new byte[] { 1, 2, 3, 4, 5 };

            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(report);
            _pdfService.Generate(report).Returns(expectedBytes);

            var result = await BuildSut().HandleAsync(Query(), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEquivalentTo(expectedBytes);
            _pdfService.Received(1).Generate(report);
        }
    }
}
