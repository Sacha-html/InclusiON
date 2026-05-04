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
    public class CreateReportCommandHandlerTests
    {
        private readonly IReportsRepository       _reportsRepo  = Substitute.For<IReportsRepository>();
        private readonly IPersonsRepository       _personsRepo  = Substitute.For<IPersonsRepository>();
        private readonly IProfessionalsRepository _prosRepo     = Substitute.For<IProfessionalsRepository>();
        private readonly IUnitOfWork              _uow          = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider        _dateTime     = Substitute.For<IDateTimeProvider>();

        private CreateReportCommandHandler BuildSut() =>
            new(_reportsRepo, _personsRepo, _prosRepo, _uow,
                NullLogger<CreateReportCommandHandler>.Instance, _dateTime);

        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly Guid ProfId   = Guid.NewGuid();

        private static CreateReportCommand Cmd() => new(
            PersonId: PersonId,
            ProfessionalId: ProfId,
            Title: "Reporte inicial",
            Content: "Contenido",
            ReportTypeId: 1,
            ReportDate: DateTime.UtcNow,
            PeriodStartDate: null,
            PeriodEndDate: null,
            AchievedGoals: null,
            AreasToReinforce: null,
            FutureRecommendations: null,
            NextObjectives: null);

        private static PersonWithDisability APerson() => new() { Id = PersonId };

        private static Professional AProfessional() => new() { Id = ProfId };

        private static Report ACreatedReport() => new()
        {
            Id             = 99,
            PersonId       = PersonId,
            ProfessionalId = ProfId,
            Title          = "Reporte inicial",
            Content        = "Contenido",
            ReportTypeId   = 1,
            ReportDate     = DateTime.UtcNow,
            Status         = ReportStatus.Draft,
            IsActive       = true,
            CreatedAt      = DateTime.UtcNow,
        };

        // ── Persona no encontrada ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_PersonNotFound_ReturnsPersonNotFound()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns((PersonWithDisability?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        // ── Profesional no encontrado ────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfessionalNotFound_ReturnsProfessionalNotFound()
        {
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(APerson());
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotFound);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidCommand_CreatesReportInDraftAndSaves()
        {
            var now     = DateTime.UtcNow;
            var created = ACreatedReport();
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(APerson());
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(AProfessional());
            _dateTime.UtcNow.Returns(now);
            _reportsRepo.CreateAsync(Arg.Any<Report>(), Arg.Any<CancellationToken>()).Returns(created);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.Id.Should().Be(99);
            result.Data.Status.Should().Be(ReportStatus.Draft);
            await _reportsRepo.Received(1).CreateAsync(
                Arg.Is<Report>(r => r.Status == ReportStatus.Draft && r.IsActive && r.CreatedAt == now),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
