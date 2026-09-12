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
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Persons
{
    public class CreatePersonWithTutorCommandHandlerTests
    {
        private readonly IPersonsRepository        _personsRepo   = Substitute.For<IPersonsRepository>();
        private readonly IFamilyRepository        _familyRepo    = Substitute.For<IFamilyRepository>();
        private readonly IAssignmentsRepository   _assignRepo    = Substitute.For<IAssignmentsRepository>();
        private readonly IProfessionalsRepository _prosRepo      = Substitute.For<IProfessionalsRepository>();
        private readonly IIdentityService         _identity      = Substitute.For<IIdentityService>();
        private readonly IUnitOfWork              _uow           = Substitute.For<IUnitOfWork>();
        private readonly IBackgroundJobRepository _bgJobs        = Substitute.For<IBackgroundJobRepository>();
        private readonly IDateTimeProvider        _dateTime      = Substitute.For<IDateTimeProvider>();
        private readonly IRoadmapInitializer      _roadmapInit   = Substitute.For<IRoadmapInitializer>();
        private readonly IRealTimeNotifier        _notifier      = Substitute.For<IRealTimeNotifier>();

        private static readonly Guid ClassroomId = Guid.NewGuid();
        private static readonly Guid ProfessionalId = Guid.NewGuid();
        private static readonly Guid ProfessionalUserId = Guid.NewGuid();

        private CreatePersonWithTutorCommandHandler BuildSut() =>
            new(_personsRepo, _familyRepo, _assignRepo, _prosRepo, _identity, _uow, _bgJobs,
                NullLogger<CreatePersonWithTutorCommandHandler>.Instance, _dateTime, _roadmapInit, _notifier);

        private static CreatePersonWithTutorCommand Cmd(string? doc = "12345678", Guid? classroomId = null) =>
            new(
                FirstName: "Lucas",
                LastName: "Pérez",
                DocumentNumber: doc,
                BirthDate: new DateTime(2005, 5, 10),
                PhotoUrl: null,
                TutorFirstName: "María",
                TutorLastName: "Pérez",
                TutorEmail: "maria.perez@example.com",
                TutorDocumentNumber: "87654321",
                TutorPhone: "123456789",
                TutorRelationship: "Madre",
                ClassroomId: classroomId ?? ClassroomId
            );

        private void SetupSuccess()
        {
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
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
                Arg.Is<string>(url => url.StartsWith("/#/pro/persons/")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_NoClassroomId_CreatesPersonWithoutAssignmentOrNotification()
        {
            var cmd = Cmd(classroomId: Guid.Empty) with { ClassroomId = null };
            SetupSuccess();

            var result = await BuildSut().HandleAsync(cmd, default);

            result.Success.Should().BeTrue();
            await _personsRepo.Received(1).CreateAsync(
                Arg.Is<PersonWithDisability>(person => !person.ProfessionalPersons.Any()),
                Arg.Any<CancellationToken>());
            await _notifier.DidNotReceiveWithAnyArgs().NotifyUserAsync(default!, default!, default!, default!, default);
            await _assignRepo.DidNotReceiveWithAnyArgs().GetClassroomByIdAsync(default, default);
            await _prosRepo.DidNotReceiveWithAnyArgs().GetByIdAsync(default, default);
        }

        [Fact]
        public async Task HandleAsync_ClassroomNotFound_ReturnsNotFound()
        {
            _assignRepo.GetClassroomByIdAsync(ClassroomId, Arg.Any<CancellationToken>()).Returns((Classroom?)null);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }
    }
}
