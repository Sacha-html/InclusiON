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
    public class SubmitReportCommandHandlerTests
    {
        private readonly IReportsRepository       _reportsRepo = Substitute.For<IReportsRepository>();
        private readonly IProfessionalsRepository _prosRepo    = Substitute.For<IProfessionalsRepository>();
        private readonly IUnitOfWork              _uow         = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider        _dateTime    = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService       _encryption  = Substitute.For<IEncryptionService>();

        private SubmitReportCommandHandler BuildSut() =>
            new(_reportsRepo, _prosRepo, _uow,
                NullLogger<SubmitReportCommandHandler>.Instance, _dateTime, _encryption);

        private static readonly Guid ProfId      = Guid.NewGuid();
        private static readonly Guid OtherProfId = Guid.NewGuid();

        private static SubmitReportCommand Cmd(Guid? profId = null) =>
            new(ReportId: 1, ProfessionalId: profId ?? ProfId);

        private static Professional AProfessional(Guid? id = null) =>
            new() { Id = id ?? ProfId };

        private static Report AReport(ReportStatus status = ReportStatus.Draft) =>
            new()
            {
                Id             = 1,
                ProfessionalId = ProfId,
                Status         = status,
                IsActive       = true,
                ReportDate     = DateTime.UtcNow,
                CreatedAt      = DateTime.UtcNow,
                CreatedBy      = ProfId,
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

        // ── Autorización ─────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfessionalNotFound_ReturnsForbidden()
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AReport());
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Forbidden);
        }

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

        // ── Estado inválido ──────────────────────────────────────────────────

        [Theory]
        [InlineData(ReportStatus.Submitted)]
        [InlineData(ReportStatus.Approved)]
        [InlineData(ReportStatus.Rejected)]
        public async Task HandleAsync_StatusNotDraft_ReturnsInvalidOperation(ReportStatus status)
        {
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AReport(status));
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(AProfessional());

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InvalidOperation);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_Draft_SetsStatusSubmittedAndSaves()
        {
            var report = AReport(ReportStatus.Draft);
            var now    = DateTime.UtcNow;
            _reportsRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(report);
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(AProfessional());
            _dateTime.UtcNow.Returns(now);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            report.Status.Should().Be(ReportStatus.Submitted);
            report.UpdatedAt.Should().Be(now);
            await _reportsRepo.Received(1).UpdateAsync(report, Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
