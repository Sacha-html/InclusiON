using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.Application.UseCases.Professionals.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Professionals
{
    public class RegisterProfessionalCommandHandlerTests
    {
        private readonly IProfessionalsRepository     _repo           = Substitute.For<IProfessionalsRepository>();
        private readonly IIdentityService             _identity       = Substitute.For<IIdentityService>();
        private readonly IUniquenessValidationService _uniqueness     = Substitute.For<IUniquenessValidationService>();
        private readonly IBackgroundJobRepository     _backgroundJobs = Substitute.For<IBackgroundJobRepository>();
        private readonly IUnitOfWork                  _uow            = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider            _dateTime       = Substitute.For<IDateTimeProvider>();

        public RegisterProfessionalCommandHandlerTests()
        {
            _uniqueness.CheckDocumentAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(DocumentCheckResult.Available());
            _uniqueness.CheckEmailAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<Guid?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(EmailCheckResult.Available());
            _uniqueness.CheckLicenseNumberAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(LicenseCheckResult.Available());
            _uow.ExecuteInTransactionAsync(Arg.Any<Func<CancellationToken, Task>>(), Arg.Any<CancellationToken>())
                .Returns(ci => ((Func<CancellationToken, Task>)ci[0])(default));
        }

        private RegisterProfessionalCommandHandler BuildSut() =>
            new(_repo, _identity, _uniqueness, _backgroundJobs, _uow,
                NullLogger<RegisterProfessionalCommandHandler>.Instance, _dateTime);

        private static RegisterProfessionalCommand Cmd(
            string email = "pro@test.com",
            string? docNumber = "12345678",
            string? licenseNumber = "PSY-001") =>
            new("Carlos", "López", docNumber, "555-1234",
                "Psicología", licenseNumber, new DateTime(1985, 3, 15),
                email, InstitutionId: null);

        private static (bool Succeeded, IEnumerable<string> Errors) SuccessResult() =>
            (true, Array.Empty<string>());

        [Fact]
        public async Task DuplicateDocument_ReturnsConflict()
        {
            _uniqueness.CheckDocumentAsync("12345678", null, Arg.Any<CancellationToken>())
                .Returns(DocumentCheckResult.Taken("Familiar", false, "El DNI 12345678 ya se encuentra registrado en el sistema (Familiar)."));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
        }

        [Fact]
        public async Task EmailAlreadyExists_ReturnsConflict()
        {
            _uniqueness.CheckEmailAsync("pro@test.com", null, null, false, Arg.Any<CancellationToken>())
                .Returns(EmailCheckResult.Taken("Este email ya se encuentra registrado en el sistema."));

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.EmailAlreadyExists);
        }

        [Fact]
        public async Task DuplicateLicenseNumber_ReturnsConflict()
        {
            _uniqueness.CheckLicenseNumberAsync("PSY-001", null, Arg.Any<CancellationToken>())
                .Returns(LicenseCheckResult.Taken("Esta matrícula ya se encuentra registrada en el sistema."));

            var result = await BuildSut().HandleAsync(Cmd(licenseNumber: "PSY-001"), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.LicenseNumberAlreadyExists);
        }

        [Fact]
        public async Task CreateUserFails_ReturnsInternalError()
        {
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                .Returns((false, (IEnumerable<string>)new[] { "Password too weak" }));
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.InternalError);
        }

        [Fact]
        public async Task ValidCommand_CreatesProfessionalPendingAndSaves()
        {
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                .Returns(SuccessResult());
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.StatusName.Should().Be("Pendiente");
            result.Data.FirstName.Should().Be("Carlos");
            await _repo.Received(1).CreateAsync(
                Arg.Is<Professional>(p => p.Email == "pro@test.com"),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _identity.Received(1).AddToRoleAsync(Arg.Any<User>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ValidCommand_NullDocument_SkipsDocumentCheck()
        {
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                .Returns(SuccessResult());
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(docNumber: null), default);

            result.Success.Should().BeTrue();
            await _uniqueness.DidNotReceive().CheckDocumentAsync(
                Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        }
    }
}
