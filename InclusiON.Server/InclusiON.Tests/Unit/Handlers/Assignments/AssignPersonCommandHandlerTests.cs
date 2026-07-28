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
    public class AssignPersonCommandHandlerTests
    {
        private readonly IAssignmentsRepository   _assignRepo  = Substitute.For<IAssignmentsRepository>();
        private readonly IProfessionalsRepository _prosRepo    = Substitute.For<IProfessionalsRepository>();
        private readonly IPersonsRepository       _personsRepo = Substitute.For<IPersonsRepository>();
        private readonly IUnitOfWork              _uow         = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider        _dateTime    = Substitute.For<IDateTimeProvider>();

        private AssignPersonCommandHandler BuildSut() =>
            new(_assignRepo, _prosRepo, _personsRepo, _uow, _dateTime);

        private static readonly Guid ProfId   = Guid.NewGuid();
        private static readonly Guid PersonId = Guid.NewGuid();

        private static AssignPersonCommand Cmd() =>
            new(ProfId, PersonId, IsPrimaryProfessional: true, CanSuperviseLogin: false);

        private static Professional ApprovedPro() => new()
        {
            Id = ProfId, Status = ProfessionalStatusEnum.Approved,
            User = new User { IsActive = true }, ProfessionalInstitutions = [],
        };

        private static PersonWithDisability APerson() => new()
        {
            Id = PersonId, UserId = Guid.NewGuid(),
            User = new User { IsActive = true },
        };

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

        // ── Persona no encontrada ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_PersonNotFound_ReturnsPersonNotFound()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns((PersonWithDisability?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        // ── Asignación ya activa ─────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AlreadyActiveAssignment_ReturnsConflict()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(APerson());
            _assignRepo.GetAssignmentAsync(ProfId, PersonId, Arg.Any<CancellationToken>())
                       .Returns(new ProfessionalPerson { IsActive = true });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DuplicateEntry);
        }

        // ── Happy path: nueva asignación ─────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NewAssignment_CreatesAndSaves()
        {
            var person = APerson();
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _personsRepo.GetByIdAsync(PersonId, Arg.Any<CancellationToken>()).Returns(person);
            _assignRepo.GetAssignmentAsync(ProfId, PersonId, Arg.Any<CancellationToken>())
                       .Returns((ProfessionalPerson?)null);
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            await _assignRepo.Received(1).CreateAssignmentAsync(
                Arg.Is<ProfessionalPerson>(a => a.ProfessionalId == ProfId && a.IsActive),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
