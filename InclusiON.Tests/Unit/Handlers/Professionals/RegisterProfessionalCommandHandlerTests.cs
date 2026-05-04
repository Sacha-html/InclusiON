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
        private readonly IProfessionalsRepository _repo = Substitute.For<IProfessionalsRepository>();
        private readonly IIdentityService _identity = Substitute.For<IIdentityService>();
        private readonly IEmailService _email = Substitute.For<IEmailService>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();

        private RegisterProfessionalCommandHandler BuildSut() =>
            new(_repo, _identity, _email, _uow,
                NullLogger<RegisterProfessionalCommandHandler>.Instance, _dateTime);

        private static RegisterProfessionalCommand Cmd(
            string email = "pro@test.com",
            string? docNumber = "12345678") =>
            new("Carlos", "López", docNumber, "555-1234",
                "Psicología", "PSY-001", new DateTime(1985, 3, 15),
                email, InstitutionId: null);

        private static (bool Succeeded, IEnumerable<string> Errors) SuccessResult() =>
            (true, Array.Empty<string>());

        [Fact]
        public async Task DuplicateDocument_ReturnsConflict()
        {
            _repo.ExistsDocumentAsync("12345678", null, Arg.Any<CancellationToken>())
                .Returns(true);

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DocumentAlreadyExists);
        }

        [Fact]
        public async Task EmailAlreadyExists_ReturnsConflict()
        {
            _repo.ExistsDocumentAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(false);
            _identity.FindByEmailAsync("pro@test.com")
                .Returns(new User { Email = "pro@test.com" });

            var result = await BuildSut().HandleAsync(Cmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.EmailAlreadyExists);
        }

        [Fact]
        public async Task CreateUserFails_ReturnsInternalError()
        {
            _repo.ExistsDocumentAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(false);
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
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
            _repo.ExistsDocumentAsync(Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
                .Returns(false);
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
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
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                .Returns(SuccessResult());
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            var result = await BuildSut().HandleAsync(Cmd(docNumber: null), default);

            result.Success.Should().BeTrue();
            await _repo.DidNotReceive().ExistsDocumentAsync(
                Arg.Any<string?>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>());
        }
    }
}
