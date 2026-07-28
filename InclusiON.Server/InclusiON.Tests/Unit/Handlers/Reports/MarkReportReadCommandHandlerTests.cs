using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Commands;
using InclusiON.Application.UseCases.Reports.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Reports
{
    public class MarkReportReadCommandHandlerTests
    {
        private readonly IReportsRepository _reportsRepo = Substitute.For<IReportsRepository>();
        private readonly IUnitOfWork        _uow         = Substitute.For<IUnitOfWork>();

        private MarkReportReadCommandHandler BuildSut() =>
            new(_reportsRepo, _uow,
                NullLogger<MarkReportReadCommandHandler>.Instance);

        private static MarkReportReadCommand Cmd() => new(ReportId: 1);

        private static Report AReport(ReportStatus status = ReportStatus.Approved, bool isRead = false) => new()
        {
            Id             = 1,
            ProfessionalId = Guid.NewGuid(),
            Status         = status,
            IsActive       = true,
            IsReadByFamily = isRead,
            ReportDate     = DateTime.UtcNow,
            CreatedAt      = DateTime.UtcNow,
        };

        // ── Reporte no encontrado ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ReportNotFound_ReturnsReportNotFound()
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Report?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ReportNotFound);
        }

        // ── Estado no aprobado ───────────────────────────────────────────────

        [Theory]
        [InlineData(ReportStatus.Draft)]
        [InlineData(ReportStatus.Submitted)]
        [InlineData(ReportStatus.Rejected)]
        public async Task HandleAsync_StatusNotApproved_ReturnsInvalidOperation(ReportStatus status)
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AReport(status));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidOperation);
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        // ── Idempotente: ya leído ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AlreadyRead_SucceedsWithoutSaving()
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>())
                        .Returns(AReport(ReportStatus.Approved, isRead: true));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            // No debe guardar nada — ya estaba leído
            await _reportsRepo.DidNotReceive().UpdateAsync(Arg.Any<Report>(), Arg.Any<CancellationToken>());
            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_UnreadApprovedReport_SetsIsReadAndSaves()
        {
            var report = AReport(ReportStatus.Approved, isRead: false);
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(report);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            report.IsReadByFamily.Should().BeTrue();
            await _reportsRepo.Received(1).UpdateAsync(report, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
