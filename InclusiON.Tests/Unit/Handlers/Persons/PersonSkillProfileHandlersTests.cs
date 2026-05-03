using FluentAssertions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Repositories.Base;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.Application.UseCases.Persons.Handlers;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;

namespace InclusiON.Tests.Unit.Handlers.Persons
{
    // ════════════════════════════════════════════════════════════════════════════
    // GetPersonProfessionalsQueryHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class GetPersonProfessionalsQueryHandlerTests
    {
        private readonly IAssignmentsRepository _assignments = Substitute.For<IAssignmentsRepository>();
        private GetPersonProfessionalsQueryHandler BuildSut() => new(_assignments);

        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly Guid ProfId   = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_NoProfessionals_ReturnsEmptyList()
        {
            _assignments.GetProfessionalsByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns(new List<ProfessionalPerson>());

            var result = await BuildSut().HandleAsync(new GetPersonProfessionalsQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_WithProfessional_MapsProfessionalFields()
        {
            var pp = new ProfessionalPerson
            {
                ProfessionalId        = ProfId,
                PersonId              = PersonId,
                IsPrimaryProfessional = true,
                CanSuperviseLogin     = true,
                IsActive              = true,
                AssignedAt            = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                Professional          = new Professional { FirstName = "Ana", LastName = "Gómez" }
            };

            _assignments.GetProfessionalsByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                        .Returns(new List<ProfessionalPerson> { pp });

            var result = await BuildSut().HandleAsync(new GetPersonProfessionalsQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);

            var item = result.Data![0];
            item.ProfessionalId.Should().Be(ProfId);
            item.PersonId.Should().Be(PersonId);
            item.PersonFirstName.Should().Be("Ana");
            item.PersonLastName.Should().Be("Gómez");
            item.PersonFullName.Should().Be("Ana Gómez");
            item.IsPrimaryProfessional.Should().BeTrue();
            item.CanSuperviseLogin.Should().BeTrue();
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // GetPersonRepresentativesQueryHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class GetPersonRepresentativesQueryHandlerTests
    {
        private readonly IFamilyRepository _family = Substitute.For<IFamilyRepository>();
        private GetPersonRepresentativesQueryHandler BuildSut() => new(_family);

        private static readonly Guid PersonId = Guid.NewGuid();
        private static readonly Guid FamilyId = Guid.NewGuid();

        [Fact]
        public async Task HandleAsync_NoRepresentatives_ReturnsEmptyList()
        {
            _family.GetPersonRepresentativesByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                   .Returns(new List<PersonRepresentative>());

            var result = await BuildSut().HandleAsync(new GetPersonRepresentativesQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task HandleAsync_WithRepresentative_MapsAllFields()
        {
            var createdAt = new DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc);
            var pr = new PersonRepresentative
            {
                PersonId        = PersonId,
                RepresentativeId= FamilyId,
                Relationship    = "Madre",
                IsPrimary       = true,
                IsActive        = true,
                CreatedAt       = createdAt,
                Representative  = new FamilyRepresentative { FirstName = "María", LastName = "López" }
            };

            _family.GetPersonRepresentativesByPersonIdAsync(PersonId, Arg.Any<CancellationToken>())
                   .Returns(new List<PersonRepresentative> { pr });

            var result = await BuildSut().HandleAsync(new GetPersonRepresentativesQuery(PersonId), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);

            var item = result.Data![0];
            item.PersonId.Should().Be(PersonId);
            item.RepresentativeId.Should().Be(FamilyId);
            item.RepresentativeFullName.Should().Be("María López");
            item.Relationship.Should().Be("Madre");
            item.IsPrimary.Should().BeTrue();
            item.IsActive.Should().BeTrue();
            item.CreatedAt.Should().Be(createdAt);
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // GetPersonSkillProfileQueryHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class GetPersonSkillProfileQueryHandlerTests
    {
        private readonly IPersonsRepository _persons = Substitute.For<IPersonsRepository>();
        private GetPersonSkillProfileQueryHandler BuildSut() => new(_persons);

        private static readonly Guid PersonId = Guid.NewGuid();

        private static PersonSkillProfile AProfile(bool isActive = true) => new()
        {
            PersonId   = PersonId,
            SkillAreaId = 1,
            IsActive   = isActive,
            AssignedAt = DateTime.UtcNow,
            SkillArea  = new SkillArea { Id = 1, Name = "Comunicación", Color = "#FF0000", Icon = "chat" }
        };

        [Fact]
        public async Task HandleAsync_AllFalse_CallsActiveOnly()
        {
            _persons.GetSkillProfileAsync(PersonId, activeOnly: true, Arg.Any<CancellationToken>())
                    .Returns(new List<PersonSkillProfile>());

            await BuildSut().HandleAsync(new GetPersonSkillProfileQuery(PersonId, All: false), default);

            await _persons.Received(1).GetSkillProfileAsync(PersonId, activeOnly: true, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_AllTrue_CallsWithActiveOnlyFalse()
        {
            _persons.GetSkillProfileAsync(PersonId, activeOnly: false, Arg.Any<CancellationToken>())
                    .Returns(new List<PersonSkillProfile>());

            await BuildSut().HandleAsync(new GetPersonSkillProfileQuery(PersonId, All: true), default);

            await _persons.Received(1).GetSkillProfileAsync(PersonId, activeOnly: false, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_WithProfile_MapsSkillAreaFields()
        {
            _persons.GetSkillProfileAsync(PersonId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                    .Returns(new List<PersonSkillProfile> { AProfile() });

            var result = await BuildSut().HandleAsync(new GetPersonSkillProfileQuery(PersonId, false), default);

            result.Success.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data![0].SkillAreaId.Should().Be(1);
            result.Data[0].SkillAreaName.Should().Be("Comunicación");
            result.Data[0].Color.Should().Be("#FF0000");
            result.Data[0].Icon.Should().Be("chat");
            result.Data[0].IsActive.Should().BeTrue();
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // AddSkillAreaCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class AddSkillAreaCommandHandlerTests
    {
        private readonly IPersonsRepository             _persons    = Substitute.For<IPersonsRepository>();
        private readonly IReadOnlyRepository<SkillArea> _skillAreas = Substitute.For<IReadOnlyRepository<SkillArea>>();
        private readonly IUnitOfWork                    _uow        = Substitute.For<IUnitOfWork>();

        private AddSkillAreaCommandHandler BuildSut() => new(_persons, _skillAreas, _uow);

        private static readonly Guid PersonId   = Guid.NewGuid();
        private const int            SkillAreaId = 5;

        private static SkillArea AnArea() =>
            new() { Id = SkillAreaId, Name = "Autonomía", Color = "#00FF00", Icon = "star", IsActive = true };

        // ── SkillArea no encontrada ─────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_SkillAreaNotFound_ReturnsNotFound()
        {
            _skillAreas.GetByIdAsync(SkillAreaId, Arg.Any<CancellationToken>())
                       .Returns((SkillArea?)null);

            var result = await BuildSut().HandleAsync(new AddSkillAreaCommand(PersonId, SkillAreaId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        // ── Área ya activa ──────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AreaAlreadyActive_ReturnsConflict()
        {
            _skillAreas.GetByIdAsync(SkillAreaId, Arg.Any<CancellationToken>())
                       .Returns(AnArea());
            _persons.GetSkillProfileEntryAsync(PersonId, SkillAreaId, Arg.Any<CancellationToken>())
                    .Returns(new PersonSkillProfile { SkillAreaId = SkillAreaId, IsActive = true });

            var result = await BuildSut().HandleAsync(new AddSkillAreaCommand(PersonId, SkillAreaId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.Conflict);
        }

        // ── Área inactiva → reactivar ───────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_AreaInactive_ReactivatesEntry()
        {
            var existing = new PersonSkillProfile
            {
                PersonId   = PersonId,
                SkillAreaId = SkillAreaId,
                IsActive   = false,
                AssignedAt = DateTime.UtcNow.AddDays(-10)
            };

            _skillAreas.GetByIdAsync(SkillAreaId, Arg.Any<CancellationToken>())
                       .Returns(AnArea());
            _persons.GetSkillProfileEntryAsync(PersonId, SkillAreaId, Arg.Any<CancellationToken>())
                    .Returns(existing);

            var result = await BuildSut().HandleAsync(new AddSkillAreaCommand(PersonId, SkillAreaId), default);

            result.Success.Should().BeTrue();
            existing.IsActive.Should().BeTrue();
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
            await _persons.DidNotReceive().AddSkillProfileEntryAsync(Arg.Any<PersonSkillProfile>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task HandleAsync_AreaInactive_Reactivate_ReturnsReactivatedMessage()
        {
            var existing = new PersonSkillProfile
            {
                PersonId    = PersonId,
                SkillAreaId = SkillAreaId,
                IsActive    = false
            };

            _skillAreas.GetByIdAsync(SkillAreaId, Arg.Any<CancellationToken>())
                       .Returns(AnArea());
            _persons.GetSkillProfileEntryAsync(PersonId, SkillAreaId, Arg.Any<CancellationToken>())
                    .Returns(existing);

            var result = await BuildSut().HandleAsync(new AddSkillAreaCommand(PersonId, SkillAreaId), default);

            result.Message.Should().Contain("reactivada");
        }

        // ── Entrada nueva ───────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_NoEntry_CreatesNewEntry()
        {
            _skillAreas.GetByIdAsync(SkillAreaId, Arg.Any<CancellationToken>())
                       .Returns(AnArea());
            _persons.GetSkillProfileEntryAsync(PersonId, SkillAreaId, Arg.Any<CancellationToken>())
                    .Returns((PersonSkillProfile?)null);

            var result = await BuildSut().HandleAsync(new AddSkillAreaCommand(PersonId, SkillAreaId), default);

            result.Success.Should().BeTrue();
            result.Data!.SkillAreaId.Should().Be(SkillAreaId);
            result.Data.SkillAreaName.Should().Be("Autonomía");
            result.Data.IsActive.Should().BeTrue();

            await _persons.Received(1).AddSkillProfileEntryAsync(
                Arg.Is<PersonSkillProfile>(p => p.PersonId == PersonId && p.SkillAreaId == SkillAreaId && p.IsActive),
                Arg.Any<CancellationToken>());
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }

    // ════════════════════════════════════════════════════════════════════════════
    // DeactivateSkillAreaCommandHandler
    // ════════════════════════════════════════════════════════════════════════════

    public class DeactivateSkillAreaCommandHandlerTests
    {
        private readonly IPersonsRepository _persons = Substitute.For<IPersonsRepository>();
        private readonly IUnitOfWork        _uow     = Substitute.For<IUnitOfWork>();

        private DeactivateSkillAreaCommandHandler BuildSut() => new(_persons, _uow);

        private static readonly Guid PersonId   = Guid.NewGuid();
        private const int            SkillAreaId = 3;

        // ── Perfil no encontrado ────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfileNotFound_ReturnsNotFound()
        {
            _persons.GetSkillProfileEntryAsync(PersonId, SkillAreaId, Arg.Any<CancellationToken>())
                    .Returns((PersonSkillProfile?)null);

            var result = await BuildSut().HandleAsync(new DeactivateSkillAreaCommand(PersonId, SkillAreaId), default);

            result.Success.Should().BeFalse();
            result.ErrorCode.Should().Be(ErrorCode.NotFound);
        }

        [Fact]
        public async Task HandleAsync_ProfileNotFound_NeverSaves()
        {
            _persons.GetSkillProfileEntryAsync(PersonId, SkillAreaId, Arg.Any<CancellationToken>())
                    .Returns((PersonSkillProfile?)null);

            await BuildSut().HandleAsync(new DeactivateSkillAreaCommand(PersonId, SkillAreaId), default);

            await _uow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
        }

        // ── Éxito ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task HandleAsync_ProfileFound_SetsIsActiveFalse()
        {
            var profile = new PersonSkillProfile
            {
                PersonId    = PersonId,
                SkillAreaId = SkillAreaId,
                IsActive    = true,
                AssignedAt  = DateTime.UtcNow,
                SkillArea   = new SkillArea { Name = "Autonomía", Color = "#00FF00", Icon = "star" }
            };

            _persons.GetSkillProfileEntryAsync(PersonId, SkillAreaId, Arg.Any<CancellationToken>())
                    .Returns(profile);

            var result = await BuildSut().HandleAsync(new DeactivateSkillAreaCommand(PersonId, SkillAreaId), default);

            result.Success.Should().BeTrue();
            profile.IsActive.Should().BeFalse();
            result.Data!.IsActive.Should().BeFalse();
            result.Data.SkillAreaName.Should().Be("Autonomía");
            await _uow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        }
    }
}
