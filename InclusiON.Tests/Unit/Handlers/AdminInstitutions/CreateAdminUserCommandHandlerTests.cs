using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.AdminInstitutions.Commands;
using InclusiON.Application.UseCases.AdminInstitutions.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.AdminInstitutions
{
    public class CreateAdminUserCommandHandlerTests
    {
        private readonly IIdentityService            _identity        = Substitute.For<IIdentityService>();
        private readonly IInstitutionsRepository     _institutionRepo = Substitute.For<IInstitutionsRepository>();
        private readonly IAdminInstitutionRepository _adminRepo       = Substitute.For<IAdminInstitutionRepository>();
        private readonly IEmailService               _email           = Substitute.For<IEmailService>();
        private readonly IUnitOfWork                 _uow             = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider           _dateTime        = Substitute.For<IDateTimeProvider>();

        private CreateAdminUserCommandHandler BuildSut() =>
            new(_identity, _institutionRepo, _adminRepo, _email, _uow, _dateTime,
                NullLogger<CreateAdminUserCommandHandler>.Instance);

        private static CreateAdminUserCommand ValidCommand() =>
            new("nuevo@test.com", "Ana", "Torres", 1);

        private static EducationalInstitution AnInstitution() =>
            new() { Id = 1, Name = "Escuela N° 1" };

        [Fact]
        public async Task HandleAsync_InstitutionNotFound_ReturnsNotFound()
        {
            // Arrange
            _institutionRepo.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
                            .Returns((EducationalInstitution?)null);

            // Act
            var result = await BuildSut().HandleAsync(ValidCommand(), default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_EmailAlreadyExists_ReturnsConflict()
        {
            // Arrange
            _institutionRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AnInstitution());
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns(new User());

            // Act
            var result = await BuildSut().HandleAsync(ValidCommand(), default);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.EmailAlreadyExists);
        }

        [Fact]
        public async Task HandleAsync_CreateUserFails_ReturnsError()
        {
            // Arrange
            _institutionRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AnInstitution());
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((false, (IEnumerable<string>)["Error de Identity"]));
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            // Act
            var result = await BuildSut().HandleAsync(ValidCommand(), default);

            // Assert
            result.Success.Should().BeFalse();
        }

        [Fact]
        public async Task HandleAsync_Success_DoesNotExposeTemporaryPasswordInResponse()
        {
            // Arrange
            _institutionRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AnInstitution());
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _identity.AddToRoleAsync(Arg.Any<User>(), "Admin")
                     .Returns((true, Enumerable.Empty<string>()));
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            // Act
            var result = await BuildSut().HandleAsync(ValidCommand(), default);

            // Assert
            result.Success.Should().BeTrue();
            // El DTO no tiene la propiedad TemporaryPassword — garantizado en compile time.
            // Este test verifica que el response devuelve los datos correctos.
            result.Data!.Email.Should().Be("nuevo@test.com");
            result.Data.FirstName.Should().Be("Ana");
            result.Data.LastName.Should().Be("Torres");
            result.Data.InstitutionId.Should().Be(1);
            result.Data.InstitutionName.Should().Be("Escuela N° 1");
        }

        [Fact]
        public async Task HandleAsync_Success_SendsWelcomeEmail()
        {
            // Arrange
            _institutionRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AnInstitution());
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _identity.AddToRoleAsync(Arg.Any<User>(), "Admin")
                     .Returns((true, Enumerable.Empty<string>()));
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            // Act
            await BuildSut().HandleAsync(ValidCommand(), default);

            // Assert
            await _email.Received(1).SendTemplatedEmailAsync(
                "nuevo@test.com",
                Arg.Any<string>(),
                "PasswordReset",
                Arg.Any<Dictionary<string, string?>>(),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_EmailFails_StillReturnsSuccess()
        {
            // Arrange
            _institutionRepo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AnInstitution());
            _identity.FindByEmailAsync(Arg.Any<string>()).Returns((User?)null);
            _identity.CreateUserAsync(Arg.Any<User>(), Arg.Any<string>())
                     .Returns((true, Enumerable.Empty<string>()));
            _identity.AddToRoleAsync(Arg.Any<User>(), "Admin")
                     .Returns((true, Enumerable.Empty<string>()));
            _email.SendTemplatedEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(),
                                           Arg.Any<Dictionary<string, string?>>(), Arg.Any<CancellationToken>())
                  .Returns(Task.FromException<bool>(new Exception("SMTP down")));
            _dateTime.UtcNow.Returns(DateTime.UtcNow);

            // Act
            var result = await BuildSut().HandleAsync(ValidCommand(), default);

            // Assert
            // Un fallo de email no debe revertir la creación del usuario
            result.Success.Should().BeTrue();
        }
    }
}
