using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Commands;
using InclusiON.Application.UseCases.Reports.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Reports
{
    public class RejectReportCommandHandlerTests
    {
        private readonly IReportsRepository       _reportsRepo  = Substitute.For<IReportsRepository>();
        private readonly IProfessionalsRepository _prosRepo     = Substitute.For<IProfessionalsRepository>();
        private readonly IEmailService            _emailService = Substitute.For<IEmailService>();
        private readonly IUnitOfWork              _uow          = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider        _dateTime     = Substitute.For<IDateTimeProvider>();

        private RejectReportCommandHandler BuildSut() =>
            new(_reportsRepo, _prosRepo, _emailService, _uow,
                NullLogger<RejectReportCommandHandler>.Instance, _dateTime);

        private static readonly Guid AdminId = Guid.NewGuid();
        private static readonly Guid ProfId  = Guid.NewGuid();

        private static RejectReportCommand Cmd(string comment = "Falta información") =>
            new(ReportId: 1, AdminUserId: AdminId, Comment: comment);

        private static Report AReport(ReportStatus status = ReportStatus.Submitted) => new()
        {
            Id             = 1,
            ProfessionalId = ProfId,
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

        // ── Comentario vacío ─────────────────────────────────────────────────

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task HandleAsync_EmptyComment_ReturnsInvalidOperation(string? comment)
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AReport());

            var cmd    = new RejectReportCommand(ReportId: 1, AdminUserId: AdminId, Comment: comment!);
            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidOperation);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_Submitted_SetsStatusRejectedAndSaves()
        {
            var report = AReport(ReportStatus.Submitted);
            var now    = DateTime.UtcNow;
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(report);
            _dateTime.UtcNow.Returns(now);
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(Cmd("  Corregir datos  "), default);

            result.Success.Should().BeTrue();
            report.Status.Should().Be(ReportStatus.Rejected);
            report.AdminComment.Should().Be("Corregir datos");
            report.UpdatedAt.Should().Be(now);
            await _reportsRepo.Received(1).UpdateAsync(report, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
