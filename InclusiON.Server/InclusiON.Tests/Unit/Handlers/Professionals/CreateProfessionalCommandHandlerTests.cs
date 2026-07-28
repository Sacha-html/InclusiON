using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.Application.UseCases.Professionals.Handlers;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Professionals
{
    public class CreateProfessionalCommandHandlerTests
    {
        private readonly IProfessionalsRepository _prosRepo  = Substitute.For<IProfessionalsRepository>();
        private readonly IIdentityService         _identity  = Substitute.For<IIdentityService>();
        private readonly IBackgroundJobRepository _backgroundJobs = Substitute.For<IBackgroundJobRepository>();
        private readonly IUnitOfWork              _uow       = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider        _dateTime  = Substitute.For<IDateTimeProvider>();

        private CreateProfessionalCommandHandler BuildSut() =>
            new(_prosRepo, _identity, _backgroundJobs, _uow,
                NullLogger<CreateProfessionalCommandHandler>.Instance, _dateTime);

        private static CreateProfessionalCommand Cmd(string? doc = null) =>
            new(FirstName: "Ana", LastName: "López",
                Email: "ana@test.com", DocumentNumber: doc);

        private void SetupSuccessfulTransaction()
        {
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _identity.AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _prosRepo.CreateAsync(Arg.Any<Professional>(), Arg.Any<CancellationToken>())
                     .Returns(ci => (Professional)ci[0]);
        }

        // ── Documento ya existe ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_DuplicateDocument_ReturnsDocumentAlreadyExists()
        {
            _prosRepo.ExistsDocumentAsync("12345678", null, Arg.Any<CancellationToken>())
                     .Returns(true);

            var result = await BuildSut().HandleAsync(Cmd(doc: "12345678"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
        }

        // ── Email ya registrado ──────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_EmailAlreadyExists_ReturnsEmailAlreadyExists()
        {
            _identity.FindByEmailAsync("ana@test.com")
                     .Returns(new User { Id = Guid.NewGuid() });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.EmailAlreadyExists);
        }

        // ── Fallo al crear usuario (Identity) ────────────────────────────────

        [Fact]
        public async Task HandleAsync_IdentityCreateFails_ReturnsValidationFailed()
        {
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((false, new[] { "Password too weak" }));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.ValidationFailed);
        }

        // ── Happy path ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ValidCommand_CreatesProfessionalWithApprovedStatus()
        {
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
            SetupSuccessfulTransaction();
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.Status.Should().Be((int)ProfessionalStatusEnum.Approved);
            result.Data.TemporaryPassword.Should().NotBeNullOrEmpty();
            await _identity.Received(1).CreateUserAsync(
                Arg.Is<User>(u => u.Email == "ana@test.com" && u.MustChangePassword),
                Arg.Any<string>());
        }
    }
}
