using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Application.UseCases.Persons.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Persons
{
    public class CreatePersonWithTutorCommandHandlerTests
    {
        private readonly IPersonsRepository           _personsRepo   = Substitute.For<IPersonsRepository>();
        private readonly IFamilyRepository            _familyRepo    = Substitute.For<IFamilyRepository>();
        private readonly IAssignmentsRepository       _assignRepo    = Substitute.For<IAssignmentsRepository>();
        private readonly IProfessionalsRepository     _prosRepo      = Substitute.For<IProfessionalsRepository>();
        private readonly IIdentityService             _identity      = Substitute.For<IIdentityService>();
        private readonly IUniquenessValidationService _uniqueness    = Substitute.For<IUniquenessValidationService>();
        private readonly IUnitOfWork                  _uow           = Substitute.For<IUnitOfWork>();
        private readonly IBackgroundJobRepository     _bgJobs        = Substitute.For<IBackgroundJobRepository>();
        private readonly IDateTimeProvider            _dateTime      = Substitute.For<IDateTimeProvider>();
        private readonly IRoadmapInitializer          _roadmapInit   = Substitute.For<IRoadmapInitializer>();
        private readonly IRealTimeNotifier            _notifier      = Substitute.For<IRealTimeNotifier>();

        private static readonly Guid ClassroomId = Guid.NewGuid();
        private static readonly Guid ProfessionalId = Guid.NewGuid();
        private static readonly Guid ProfessionalUserId = Guid.NewGuid();

        private CreatePersonWithTutorCommandHandler BuildSut() =>
            new(_personsRepo, _familyRepo, _assignRepo, _prosRepo, _identity, _uniqueness, _uow, _bgJobs,
                NullLogger<CreatePersonWithTutorCommandHandler>.Instance, _dateTime, _roadmapInit, _notifier);

        private static CreatePersonWithTutorCommand Cmd(
            string? doc = "12345678",
            string? tutorDoc = "87654321",
            string? tutorEmail = "maria.perez@example.com",
            Guid? professionalId = null,
            Guid? classroomId = null,
            Guid? existingTutorId = null) =>
            new(
                FirstName: "Lucas",
                LastName: "Pérez",
                DocumentNumber: doc,
                BirthDate: new DateTime(2005, 5, 10),
                PhotoUrl: null,
                TutorFirstName: existingTutorId.HasValue ? null : "María",
                TutorLastName: existingTutorId.HasValue ? null : "Pérez",
                TutorEmail: existingTutorId.HasValue ? null : tutorEmail,
                TutorDocumentNumber: existingTutorId.HasValue ? null : tutorDoc,
                TutorPhone: existingTutorId.HasValue ? null : "123456789",
                TutorRelationship: "Madre",
                ProfessionalId: professionalId ?? ProfessionalId,
                ClassroomId: classroomId ?? ClassroomId,
                ExistingTutorId: existingTutorId
            );

        private void SetupSuccess()
        {
            _uniqueness.CheckDocumentAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(DocumentCheckResult.Available());
            _uniqueness.CheckEmailAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(EmailCheckResult.Available());

            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _identity.AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));

            var classroom = new Classroom { Id = ClassroomId, ProfessionalId = ProfessionalId, Name = "Aula 1" };
            _assignRepo.GetClassroomByIdAsync(ClassroomId, Arg.Any<CancellationToken>()).Returns(classroom);

            var professional = new Professional { Id = ProfessionalId, UserId = ProfessionalUserId };
            _prosRepo.GetByIdAsync(ProfessionalId, Arg.Any<CancellationToken>()).Returns(professional);

            _dateTime.UtcNow.Returns(new DateTime(2026, 9, 10, 12, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public async Task HandleAsync_ValidRequest_NotifiesAssignedProfessional()
        {
            SetupSuccess();

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            await _personsRepo.Received(1).CreateAsync(
                Arg.Is<PersonWithDisability>(person => person.ProfessionalPersons.Count == 1 &&
                    person.ProfessionalPersons.Single().ProfessionalId == ProfessionalId &&
                    person.ProfessionalPersons.Single().ClassroomId == ClassroomId),
                Arg.Any<CancellationToken>());
            await _notifier.Received(1).NotifyUserAsync(
                ProfessionalUserId.ToString(),
                "🎓 Nuevo alumno asignado",
                Arg.Is<string>(msg => msg.Contains("Lucas Pérez") && msg.Contains("perfil funcional")),
                Arg.Is<string>(url => url.StartsWith("/pro/persons/")),
                Arg.Any<CancellationToken>());
            await _roadmapInit.Received(1).InitializeStudentRoadmapAsync(
                Arg.Any<Guid>(), ProfessionalUserId, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_NoClassroomId_CreatesPersonWithAssignmentAndNotifiesProfessional()
        {
            var cmd = Cmd() with { ClassroomId = null };
            SetupSuccess();

            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeTrue();
            await _personsRepo.Received(1).CreateAsync(
                Arg.Is<PersonWithDisability>(person => person.ProfessionalPersons.Count == 1 &&
                    person.ProfessionalPersons.Single().ProfessionalId == ProfessionalId &&
                    person.ProfessionalPersons.Single().ClassroomId == null &&
                    person.ProfessionalPersons.Single().IsPrimaryProfessional),
                Arg.Any<CancellationToken>());
            await _assignRepo.DidNotReceiveWithAnyArgs().GetClassroomByIdAsync(default, default);
        }

        [Fact]
        public async Task HandleAsync_ProfessionalNotFound_ReturnsNotFound()
        {
            SetupSuccess();
            _prosRepo.GetByIdAsync(ProfessionalId, Arg.Any<CancellationToken>()).Returns((Professional?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_ClassroomBelongsToAnotherProfessional_ReturnsValidationFailed()
        {
            SetupSuccess();
            var anotherProfessionalId = Guid.NewGuid();
            var classroom = new Classroom { Id = ClassroomId, ProfessionalId = anotherProfessionalId, Name = "Aula 1" };
            _assignRepo.GetClassroomByIdAsync(ClassroomId, Arg.Any<CancellationToken>()).Returns(classroom);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        [Fact]
        public async Task HandleAsync_ClassroomNotFound_ReturnsNotFound()
        {
            SetupSuccess();
            _assignRepo.GetClassroomByIdAsync(ClassroomId, Arg.Any<CancellationToken>()).Returns((Classroom?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_WhenPostCommitStepFails_ReturnsSuccessAndRunsRemainingSteps()
        {
            SetupSuccess();
            _roadmapInit
                .When(x => x.InitializeStudentRoadmapAsync(
                    Arg.Any<Guid>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>()))
                .Do(_ => throw new InvalidOperationException("roadmap unavailable"));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            await _bgJobs.Received(2).CreateAsync(
                Arg.Any<int>(), Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
            await _notifier.Received(1).NotifyUserAsync(
                ProfessionalUserId.ToString(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_WhenSaveFails_ReturnsErrorAndDoesNotRunPostCommitSteps()
        {
            SetupSuccess();
            _uow.SaveChangesAsync(Arg.Any<CancellationToken>())
                .Returns(_ => Task.FromException<int>(new InvalidOperationException("save failed")));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InternalError);
            await _roadmapInit.DidNotReceiveWithAnyArgs()
                .InitializeStudentRoadmapAsync(default, default, default);
            await _bgJobs.DidNotReceiveWithAnyArgs()
                .CreateAsync(default, default!, default, default, default);
        }

        [Fact]
        public async Task HandleAsync_ActiveStudentWithSameDniExists_ReturnsConflict()
        {
            SetupSuccess();
            _uniqueness.CheckDocumentAsync("12345678", null, Arg.Any<CancellationToken>())
                .Returns(DocumentCheckResult.Taken("Alumno", false, "El DNI 12345678 ya se encuentra registrado en el sistema (Alumno)."));

            var result = await BuildSut().HandleAsync(Cmd(doc: "12345678"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
            await _personsRepo.DidNotReceive().CreateAsync(Arg.Any<PersonWithDisability>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_InactiveStudentWithSameDniExists_ReturnsConflictAndDirectsToReactivation()
        {
            SetupSuccess();
            _uniqueness.CheckDocumentAsync("12345678", null, Arg.Any<CancellationToken>())
                .Returns(DocumentCheckResult.Taken("Alumno", true, "Ya existe un alumno inactivo registrado con el DNI 12345678. Podés reactivarlo desde el listado de Alumnos."));

            var result = await BuildSut().HandleAsync(Cmd(doc: "12345678"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
            result.Message.Should().Contain("reactivar");
            await _personsRepo.DidNotReceive().CreateAsync(Arg.Any<PersonWithDisability>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_StudentAndTutorSameDni_ReturnsConflict()
        {
            SetupSuccess();

            var result = await BuildSut().HandleAsync(Cmd(doc: "12345678", tutorDoc: "12345678"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
            result.Message.Should().Contain("no pueden ser iguales");
        }

        [Fact]
        public async Task HandleAsync_TutorEmailAlreadyExists_ReturnsConflict()
        {
            SetupSuccess();
            _uniqueness.CheckEmailAsync("maria.perez@example.com", null, null, false, Arg.Any<CancellationToken>())
                .Returns(EmailCheckResult.Taken("Este email ya se encuentra registrado en el sistema."));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.EmailAlreadyExists);
            await _personsRepo.DidNotReceive().CreateAsync(Arg.Any<PersonWithDisability>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_TutorDniExistsInAnotherRole_ReturnsConflict()
        {
            SetupSuccess();
            _uniqueness.CheckDocumentAsync("87654321", null, Arg.Any<CancellationToken>())
                .Returns(DocumentCheckResult.Taken("Profesional", false, "El DNI 87654321 ya se encuentra registrado en el sistema (Profesional)."));

            var result = await BuildSut().HandleAsync(Cmd(tutorDoc: "87654321"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
        }

        [Fact]
        public async Task HandleAsync_ExistingTutorIdProvided_LinksWithoutCreatingNewTutorUser()
        {
            SetupSuccess();
            var existingTutorId = Guid.NewGuid();
            var existingTutorUserId = Guid.NewGuid();
            _familyRepo.GetByIdAsync(existingTutorId, Arg.Any<CancellationToken>())
                .Returns(new FamilyRepresentative
                {
                    Id = existingTutorId,
                    UserId = existingTutorUserId,
                    FirstName = "María",
                    LastName = "Pérez",
                    IsActive = true,
                    Status = FamilyStatusEnum.Active,
                    User = new User { Id = existingTutorUserId, IsActive = true, Email = "maria@test.com" }
                });

            var result = await BuildSut().HandleAsync(Cmd(existingTutorId: existingTutorId), default);

            result.Success.Should().BeTrue();
            result.Data!.TutorTemporaryPassword.Should().BeNull();
            await _familyRepo.DidNotReceive().CreateAsync(Arg.Any<FamilyRepresentative>(), Arg.Any<CancellationToken>());
            await _identity.Received(1).CreateUserAsync(Arg.Any<User>(), Arg.Any<string>()); // solo el alumno
            await _personsRepo.Received(1).CreateAsync(
                Arg.Is<PersonWithDisability>(p =>
                    p.SupervisorUserId == existingTutorUserId &&
                    p.PersonRepresentatives.Single().RepresentativeId == existingTutorId),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_HomonymWithDifferentDni_CreatesNewStudent()
        {
            SetupSuccess();

            var result = await BuildSut().HandleAsync(Cmd(doc: "12345678"), default);

            result.Success.Should().BeTrue();
            await _personsRepo.Received(1).CreateAsync(
                Arg.Is<PersonWithDisability>(p => p.DocumentNumber == "12345678" && p.FirstName == "Lucas"),
                Arg.Any<CancellationToken>());
        }
    }
}
