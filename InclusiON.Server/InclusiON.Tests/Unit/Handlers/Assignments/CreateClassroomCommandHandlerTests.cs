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
    public class CreateClassroomCommandHandlerTests
    {
        private readonly IAssignmentsRepository   _assignRepo  = Substitute.For<IAssignmentsRepository>();
        private readonly IProfessionalsRepository _prosRepo    = Substitute.For<IProfessionalsRepository>();
        private readonly IPersonsRepository       _personsRepo = Substitute.For<IPersonsRepository>();
        private readonly IUnitOfWork              _uow         = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider        _dateTime    = Substitute.For<IDateTimeProvider>();

        private CreateClassroomCommandHandler BuildSut() =>
            new(_assignRepo, _prosRepo, _personsRepo, _uow, _dateTime);

        private static readonly Guid ProfId   = Guid.NewGuid();
        private static readonly Guid PersonId1 = Guid.NewGuid();
        private static readonly Guid PersonId2 = Guid.NewGuid();

        private static CreateClassroomCommand Cmd(string name, List<Guid> personIds) =>
            new(ProfId, name, personIds, IsPrimaryProfessional: true, CanSuperviseLogin: false);

        private static Professional ApprovedPro() => new()
        {
            Id = ProfId, Status = ProfessionalStatusEnum.Approved,
            User = new User { IsActive = true }, ProfessionalInstitutions = [],
        };

        private static PersonWithDisability APerson(Guid id) => new()
        {
            Id = id, UserId = Guid.NewGuid(),
            User = new User { IsActive = true },
        };

        // ── Profesional no encontrado ────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfessionalNotFound_ReturnsProfessionalNotFound()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>())
                     .Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(Cmd("Aula A", new() { PersonId1 }), default);

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

            var result = await BuildSut().HandleAsync(Cmd("Aula A", new() { PersonId1 }), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ProfessionalNotApproved);
        }

        // ── Nombre de aula vacío ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_EmptyName_ReturnsValidationFailed()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());

            var result = await BuildSut().HandleAsync(Cmd("", new() { PersonId1 }), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
            result.Message.Should().Contain("nombre");
        }

        // ── Lista de alumnos vacía ────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_EmptyPersonIds_ReturnsValidationFailed()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());

            var result = await BuildSut().HandleAsync(Cmd("Aula A", new()), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
            result.Message.Should().Contain("alumno");
        }

        // ── Alumno no encontrado ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_PersonNotFound_ReturnsPersonNotFound()
        {
            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _personsRepo.GetByIdAsync(PersonId1, Arg.Any<CancellationToken>()).Returns((PersonWithDisability?)null);

            var result = await BuildSut().HandleAsync(Cmd("Aula A", new() { PersonId1 }), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.PersonNotFound);
        }

        // ── Happy path: crea aula y asigna alumnos ────────────────────────────

        [Fact]
        public async Task HandleAsync_HappyPath_CreatesClassroomAndAssignments()
        {
            var now = DateTime.UtcNow;
            _dateTime.UtcNow.Returns(now);

            _prosRepo.GetByIdAsync(ProfId, Arg.Any<CancellationToken>()).Returns(ApprovedPro());
            _personsRepo.GetByIdAsync(PersonId1, Arg.Any<CancellationToken>()).Returns(APerson(PersonId1));
            _personsRepo.GetByIdAsync(PersonId2, Arg.Any<CancellationToken>()).Returns(APerson(PersonId2));

            _assignRepo.GetAssignmentAsync(ProfId, PersonId1, Arg.Any<CancellationToken>()).Returns((ProfessionalPerson?)null);
            _assignRepo.GetAssignmentAsync(ProfId, PersonId2, Arg.Any<CancellationToken>()).Returns((ProfessionalPerson?)null);

            var result = await BuildSut().HandleAsync(Cmd("Aula Tarde", new() { PersonId1, PersonId2 }), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(2);

            // Verificar creación de aula
            await _assignRepo.Received(1).CreateClassroomAsync(
                Arg.Is<Classroom>(c => c.Name == "Aula Tarde" && c.ProfessionalId == ProfId && c.IsActive),
                Arg.Any<CancellationToken>());

            // Verificar creación de las dos asignaciones
            await _assignRepo.Received(1).CreateAssignmentAsync(
                Arg.Is<ProfessionalPerson>(a => a.ProfessionalId == ProfId && a.PersonId == PersonId1 && a.IsActive),
                Arg.Any<CancellationToken>());
            await _assignRepo.Received(1).CreateAssignmentAsync(
                Arg.Is<ProfessionalPerson>(a => a.ProfessionalId == ProfId && a.PersonId == PersonId2 && a.IsActive),
                Arg.Any<CancellationToken>());

            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
