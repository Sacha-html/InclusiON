using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Institutions.Commands;
using InclusiON.Application.UseCases.Institutions.Handlers;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Institutions
{
    public class CreateUpdateInstitutionCommandHandlerTests
    {
        private readonly IInstitutionsRepository _repo = Substitute.For<IInstitutionsRepository>();
        private readonly IUnitOfWork _uow = Substitute.For<IUnitOfWork>();
        private readonly IDateTimeProvider _dateTime = Substitute.For<IDateTimeProvider>();
        private readonly IEncryptionService _encryption = Substitute.For<IEncryptionService>();

        private static readonly DateTime Now = new(2025, 6, 1, 12, 0, 0, DateTimeKind.Utc);

        // ── CreateInstitution ────────────────────────────────────────────

        private CreateInstitutionCommandHandler BuildCreate() =>
            new(_repo, _uow, _dateTime, _encryption);

        private static CreateInstitutionCommand CreateCmd(string name = "Colegio Test") =>
            new(name, "Av. Siempre Viva 123", "555-1234", "colegio@test.com");

        [Fact]
        public async Task Create_DuplicateName_ReturnsConflict()
        {
            _repo.ExistsByNameAsync("Colegio Test", null, Arg.Any<CancellationToken>())
                .Returns(true);

            var result = await BuildCreate().HandleAsync(CreateCmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DuplicateEntry);
        }

        [Fact]
        public async Task Create_UniqueName_CreatesAndSaves()
        {
            _repo.ExistsByNameAsync("Colegio Test", null, Arg.Any<CancellationToken>())
                .Returns(false);
            _dateTime.UtcNow.Returns(Now);
            _repo.CreateAsync(Arg.Any<EducationalInstitution>(), Arg.Any<CancellationToken>())
                .Returns(ci => ci.Arg<EducationalInstitution>());

            var result = await BuildCreate().HandleAsync(CreateCmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.Name.Should().Be("Colegio Test");
            result.Data.IsActive.Should().BeTrue();
            await _repo.Received(1).CreateAsync(Arg.Any<EducationalInstitution>(), Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        // ── UpdateInstitution ────────────────────────────────────────────

        private UpdateInstitutionCommandHandler BuildUpdate() =>
            new(_repo, _uow, _dateTime, _encryption);

        private static UpdateInstitutionCommand UpdateCmd(int id = 1, string name = "Nuevo Nombre") =>
            new(id, name, "Nueva Dir", "555-9999", "nuevo@test.com");

        private static EducationalInstitution AnInstitution(int id = 1) => new()
        {
            Id = id, Name = "Colegio Viejo", IsActive = true,
            CreatedAt = new DateTime(2024, 1, 1)
        };

        [Fact]
        public async Task Update_InstitutionNotFound_ReturnsNotFound()
        {
            _repo.GetByIdAsync(1, Arg.Any<CancellationToken>())
                .Returns((EducationalInstitution?)null);

            var result = await BuildUpdate().HandleAsync(UpdateCmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task Update_DuplicateName_ReturnsConflict()
        {
            _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(AnInstitution());
            _repo.ExistsByNameAsync("Nuevo Nombre", 1, Arg.Any<CancellationToken>()).Returns(true);

            var result = await BuildUpdate().HandleAsync(UpdateCmd(), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.DuplicateEntry);
        }

        [Fact]
        public async Task Update_Valid_UpdatesFieldsAndSaves()
        {
            var institution = AnInstitution();
            _repo.GetByIdAsync(1, Arg.Any<CancellationToken>()).Returns(institution);
            _repo.ExistsByNameAsync("Nuevo Nombre", 1, Arg.Any<CancellationToken>()).Returns(false);
            _dateTime.UtcNow.Returns(Now);

            var result = await BuildUpdate().HandleAsync(UpdateCmd(), default);

            result.Success.Should().BeTrue();
            result.Data!.Name.Should().Be("Nuevo Nombre");
            result.Data.Phone.Should().Be("555-9999");
            institution.UpdatedAt.Should().Be(Now);
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
