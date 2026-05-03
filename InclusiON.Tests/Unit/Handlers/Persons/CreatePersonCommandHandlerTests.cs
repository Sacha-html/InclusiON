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
    public class CreatePersonCommandHandlerTests
    {
        private readonly IPersonsRepository _personsRepo   = Substitute.For<IPersonsRepository>();
        private readonly IIdentityService   _identity      = Substitute.For<IIdentityService>();
        private readonly IPasswordHasher    _pwdHasher     = Substitute.For<IPasswordHasher>();
        private readonly IPinHasher         _pinHasher     = Substitute.For<IPinHasher>();
        private readonly IUnitOfWork        _uow           = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider  _dateTime      = Substitute.For<IDateTimeProvider>();

        private CreatePersonCommandHandler BuildSut() =>
            new(_personsRepo, _identity, _pwdHasher, _pinHasher, _uow,
                NullLogger<CreatePersonCommandHandler>.Instance, _dateTime);

        private static CreatePersonCommand Cmd(string? doc = null) =>
            new("Lucas", "Pérez", doc,
                BirthDate: new DateTime(2000, 1, 1), DisabilityTypeId: 1, PhotoUrl: null,
                AttentionLevel: null, CommunicationLevel: null,
                UsesAAC: false, UsesSignLanguage: false, MotorSkillLevel: null,
                InterestsAndMotivators: null, LearningStyle: null,
                AvailableResources: null, AdditionalTherapies: null,
                RequiresLargeFont: false, RequiresHighContrast: false,
                VisualNoiseSensitivity: false, SoundSensitivity: false,
                ColorBlindnessType: null,
                AutonomyLevelId: 1, LoginMethodId: 1,
                Pin: null, SupervisorUserId: null, AvatarColor: null);

        private void SetupTransaction()
        {
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _identity.AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _personsRepo.CreateAsync(Arg.Any<PersonWithDisability>(), Arg.Any<CancellationToken>())
                        .Returns(ci => (PersonWithDisability)ci[0]);
        }

        // ── Documento duplicado ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_DocumentExists_ReturnsDocumentAlreadyExists()
        {
            _personsRepo.ExistsDocumentAsync("12345678", null, Arg.Any<CancellationToken>())
                        .Returns(true);

            var result = await BuildSut().HandleAsync(Cmd(doc: "12345678"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
        }

        // ── Fallo de Identity ────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_IdentityCreateFails_ReturnsValidationFailed()
        {
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((false, new[] { "Password too weak" }));
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidCommand_CreatesPersonAndReturnsResponse()
        {
            SetupTransaction();
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            await _personsRepo.Received(1).CreateAsync(
                Arg.Is<PersonWithDisability>(p => p.FirstName == "Lucas"),
                Arg.Any<CancellationToken>());
        }
    }
}
