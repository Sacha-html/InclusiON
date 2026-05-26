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
    public class DeactivateReportCommandHandlerTests
    {
        private readonly IReportsRepository         _reportsRepo   = Substitute.For<IReportsRepository>();
        private readonly IProfessionalsRepository   _prosRepo      = Substitute.For<IProfessionalsRepository>();
        private readonly IUnitOfWork                _uow           = Substitute.For<IUnitOfWork>();

        private DeactivateReportCommandHandler BuildSut() =>
            new(_reportsRepo, _prosRepo, _uow, NullLogger<DeactivateReportCommandHandler>.Instance);

        private static readonly Guid ProfId   = Guid.NewGuid();
        private static readonly Guid OtherProfId = Guid.NewGuid();

        private static DeactivateReportCommand Cmd(Guid? profId = null) =>
            new(ReportId: 1, ProfessionalId: profId ?? ProfId);

        private static Professional AProfessional(Guid? id = null) =>
            new() { Id = id ?? ProfId };

        private static Report AReport(ReportStatus status = ReportStatus.Draft, bool isActive = true) =>
            new()
            {
                Id             = 1,
                ProfessionalId = ProfId,
                Status         = status,
                IsActive       = isActive,
                CreatedAt      = DateTime.UtcNow,
                CreatedBy      = ProfId,
            };

        // ── Reporte no encontrado ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ReportNotFound_ReturnsError()
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns((Report?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ReportNotFound);
        }

        // ── Profesional no autorizado ────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_DifferentProfessional_ReturnsForbidden()
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AReport());
            _prosRepo.GetByIdAsync(OtherProfId, Arg.Any<CancellationToken>())
                     .Returns(AProfessional(OtherProfId));

            var result = await BuildSut().HandleAsync(Cmd(OtherProfId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

        // ── Ya inactivo ──────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AlreadyInactive_ReturnsError()
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AReport(isActive: false));
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(AProfessional());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidOperation);
        }

        // ── Estado Enviado bloqueado ─────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_StatusSubmitted_ReturnsError()
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AReport(ReportStatus.Submitted));
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(AProfessional());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidOperation);
        }

        // ── Happy path — Borrador ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_Draft_DeactivatesAndSaves()
        {
            var report = AReport(ReportStatus.Draft);
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(report);
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(AProfessional());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            report.IsActive.Should().BeFalse();
            await _reportsRepo.Received(1).UpdateAsync(report, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        // ── Happy path — Aprobado ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_Approved_DeactivatesAndSaves()
        {
            var report = AReport(ReportStatus.Approved);
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(report);
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(AProfessional());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            report.IsActive.Should().BeFalse();
        }

        // ── Happy path — Rechazado ───────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_Rejected_DeactivatesAndSaves()
        {
            var report = AReport(ReportStatus.Rejected);
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(report);
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(AProfessional());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            report.IsActive.Should().BeFalse();
        }
    }
}
