using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Assignments.Commands;
using InclusiON.Application.UseCases.Assignments.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Assignments
{
    public class AssignInstitutionCommandHandlerTests
    {
        private readonly IAssignmentsRepository   _assignRepo  = Substitute.For<IAssignmentsRepository>();
        private readonly IProfessionalsRepository _prosRepo    = Substitute.For<IProfessionalsRepository>();
        private readonly IInstitutionsRepository  _instRepo    = Substitute.For<IInstitutionsRepository>();
        private readonly IUnitOfWork              _uow         = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider        _dateTime    = Substitute.For<IDateTimeProvider>();

        private AssignInstitutionCommandHandler BuildSut() =>
            new(_assignRepo, _prosRepo, _instRepo, _uow, _dateTime);

        private static readonly Guid ProfId = Guid.NewGuid();
        private const int InstId = 42;

        private static AssignInstitutionCommand Cmd() => new(ProfId, InstId);

        private static Professional ApprovedPro() => new()
        {
            Id = ProfId, Status = ProfessionalStatusEnum.Approved,
            User = new User { IsActive = true }, ProfessionalInstitutions = [],
        };

        private static EducationalInstitution AnInstitution() => new() { Id = InstId, Name = "Escuela" };

        // ── Profesional no encontrado ────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfessionalNotFound_ReturnsProfessionalNotFound()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotFound);
        }

        // ── Profesional no aprobado ──────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfessionalNotApproved_ReturnsProfessionalNotApproved()
        {
            var pro = ApprovedPro();
            pro.Status = ProfessionalStatusEnum.Pending;
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(pro);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotApproved);
        }

        // ── Institución no encontrada ────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_InstitutionNotFound_ReturnsNotFound()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _instRepo.GetByIdAsync(InstId, Arg.Any<CancellationToken>()).Returns((EducationalInstitution?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        // ── Asignación ya activa ─────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AlreadyActive_ReturnsConflict()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _instRepo.GetByIdAsync(InstId, Arg.Any<CancellationToken>()).Returns(AnInstitution());
            _assignRepo.GetInstitutionAssignmentAsync(ProfId, InstId, Arg.Any<CancellationToken>())
                       .Returns(new ProfessionalInstitution { IsActive = true });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DuplicateEntry);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NewAssignment_CreatesAndSaves()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _instRepo.GetByIdAsync(InstId, Arg.Any<CancellationToken>()).Returns(AnInstitution());
            _assignRepo.GetInstitutionAssignmentAsync(ProfId, InstId, Arg.Any<CancellationToken>())
                       .Returns((ProfessionalInstitution?)null);
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            await _assignRepo.Received(1).CreateInstitutionAssignmentAsync(
                Arg.Is<ProfessionalInstitution>(a => a.ProfessionalId == ProfId && a.IsActive),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
