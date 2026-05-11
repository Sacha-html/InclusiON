using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using Microsoft.Extensions.DependencyInjection;
using InclusiON.Application.UseCases.Reports.Commands;
using InclusiON.Application.UseCases.Reports.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Reports
{
    public class ApproveReportCommandHandlerTests
    {
        private readonly IReportsRepository    _reportsRepo  = Substitute.For<IReportsRepository>();
        private readonly IEmailService         _emailService = Substitute.For<IEmailService>();
        private readonly IUnitOfWork           _uow          = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider     _dateTime     = Substitute.For<IDateTimeProvider>();
        private readonly IServiceScopeFactory  _scopeFactory = Substitute.For<IServiceScopeFactory>();

        private ApproveReportCommandHandler BuildSut() =>
            new(_reportsRepo, _emailService, _uow,
                NullLogger<ApproveReportCommandHandler>.Instance, _dateTime, _scopeFactory);

        private static readonly Guid AdminId = Guid.NewGuid();

        private static ApproveReportCommand Cmd() => new(ReportId: 1, AdminUserId: AdminId);

        private static Report AReport(ReportStatus status = ReportStatus.Submitted) => new()
        {
            Id             = 1,
            ProfessionalId = Guid.NewGuid(),
            Status         = status,
            IsActive       = true,
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

        // ── Estado inválido ──────────────────────────────────────────────────

        [Theory]
        [InlineData(ReportStatus.Draft)]
        [InlineData(ReportStatus.Approved)]
        [InlineData(ReportStatus.Rejected)]
        public async Task HandleAsync_StatusNotSubmitted_ReturnsInvalidOperation(ReportStatus status)
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AReport(status));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidOperation);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_Submitted_SetsStatusApprovedAndSaves()
        {
            var report = AReport(ReportStatus.Submitted);
            var now    = DateTime.UtcNow;
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(report);
            _dateTime.UtcNow.Returns(now);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            report.Status.Should().Be(ReportStatus.Approved);
            report.ApprovedAt.Should().Be(now);
            report.ApprovedBy.Should().Be(AdminId);
            report.UpdatedAt.Should().Be(now);
            await _reportsRepo.Received(1).UpdateAsync(report, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
