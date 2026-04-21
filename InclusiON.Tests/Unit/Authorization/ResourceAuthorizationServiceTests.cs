using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using InclusiON.Application.Auditing;
using InclusiON.Application.Authorization;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.Infrastructure.Authorization;
using InclusiON.Tests.TestSupport;

namespace InclusiON.Tests.Authorization
{
    /// <summary>
    /// Tests de happy-path para <see cref="ResourceAuthorizationService"/> (HU-IN-172).
    /// Cubre 4 casos criticos de Fase 1: Professional con/sin asignacion, GlobalAdmin bypass,
    /// Family con vinculo activo. La matriz completa (24+ casos) queda para Fase 3.
    /// </summary>
    public class ResourceAuthorizationServiceTests : DbContextTestBase
    {
        private static IHttpContextService BuildHttpContext(Guid userId, string role, bool isGlobalAdmin = false)
        {
            var svc = Substitute.For<IHttpContextService>();
            svc.GetCurrentUserId().Returns(userId);
            svc.GetCurrentUserRole().Returns(role);
            svc.IsGlobalAdmin().Returns(isGlobalAdmin);
            svc.GetInstitutionIds().Returns(new List<int>());
            svc.GetClientIpAddress().Returns("127.0.0.1");
            svc.GetCorrelationId().Returns("test-correlation");
            return svc;
        }

        private ResourceAuthorizationService BuildSut(
            IHttpContextService http,
            out IAccessAuditLogger audit)
        {
            audit = Substitute.For<IAccessAuditLogger>();
            return new ResourceAuthorizationService(
                Db,
                http,
                audit,
                NullLogger<ResourceAuthorizationService>.Instance);
        }

        private static AccessAuditEntry BuildExpectedEntry(
            Guid userId,
            string role,
            Guid personId,
            string result,
            AccessMode mode = AccessMode.Read) => new()
            {
                UserId = userId,
                Role = role,
                AccessedPersonId = personId,
                ActionType = mode == AccessMode.Write
                    ? AccessAuditValues.Action.Update
                    : AccessAuditValues.Action.Read,
                Result = result,
                AffectedTable = "Persons",
                AffectedRecordId = personId.ToString(),
                Details = null
            };

        [Fact]
        public async Task Professional_with_active_assignment_is_allowed_and_audited_as_allowed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var professionalId = Guid.NewGuid();
            var personId = Guid.NewGuid();

            Db.Professionals.Add(new Professional { Id = professionalId, UserId = userId });
            Db.ProfessionalPersons.Add(new ProfessionalPerson
            {
                ProfessionalId = professionalId,
                PersonId = personId,
                IsActive = true
            });
            await Db.SaveChangesAsync();

            var http = BuildHttpContext(userId, nameof(IdentityRoles.Professional));
            var sut = BuildSut(http, out var audit);

            var expectedEntry = BuildExpectedEntry(
                userId, nameof(IdentityRoles.Professional), personId, AccessAuditValues.Result.Allowed);

            // Act
            var allowed = await sut.CanAccessPersonAsync(personId, AccessMode.Read);

            // Assert
            allowed.Should().BeTrue();
            await audit.Received(1).LogAsync(expectedEntry, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Professional_without_assignment_is_denied_and_audited_as_denied()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var professionalId = Guid.NewGuid();
            var otherPersonId = Guid.NewGuid();

            Db.Professionals.Add(new Professional { Id = professionalId, UserId = userId });
            // No ProfessionalPerson row — el profesional no tiene asignaciones
            await Db.SaveChangesAsync();

            var http = BuildHttpContext(userId, nameof(IdentityRoles.Professional));
            var sut = BuildSut(http, out var audit);

            var expectedEntry = BuildExpectedEntry(
                userId, nameof(IdentityRoles.Professional), otherPersonId, AccessAuditValues.Result.Denied);

            // Act
            var allowed = await sut.CanAccessPersonAsync(otherPersonId, AccessMode.Read);

            // Assert
            allowed.Should().BeFalse();
            await audit.Received(1).LogAsync(expectedEntry, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task GlobalAdmin_is_always_allowed_without_any_link()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var anyPersonId = Guid.NewGuid();
            // DB completamente vacia, no hay ProfessionalPerson ni PersonRepresentative

            var http = BuildHttpContext(userId, nameof(IdentityRoles.Admin), isGlobalAdmin: true);
            var sut = BuildSut(http, out var audit);

            var expectedEntry = BuildExpectedEntry(
                userId, nameof(IdentityRoles.Admin), anyPersonId, AccessAuditValues.Result.Allowed);

            // Act
            var allowed = await sut.CanAccessPersonAsync(anyPersonId, AccessMode.Read);

            // Assert
            allowed.Should().BeTrue("los GlobalAdmin hacen bypass de la validacion de vinculo");
            await audit.Received(1).LogAsync(expectedEntry, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Write_mode_audits_ActionType_as_Update()
        {
            // Arrange — profesional asignado, modo Write
            var userId = Guid.NewGuid();
            var professionalId = Guid.NewGuid();
            var personId = Guid.NewGuid();

            Db.Professionals.Add(new Professional { Id = professionalId, UserId = userId });
            Db.ProfessionalPersons.Add(new ProfessionalPerson
            {
                ProfessionalId = professionalId,
                PersonId = personId,
                IsActive = true
            });
            await Db.SaveChangesAsync();

            var http = BuildHttpContext(userId, nameof(IdentityRoles.Professional));
            var sut = BuildSut(http, out var audit);

            var expectedEntry = BuildExpectedEntry(
                userId, nameof(IdentityRoles.Professional), personId,
                AccessAuditValues.Result.Allowed, AccessMode.Write);

            // Act
            var allowed = await sut.CanAccessPersonAsync(personId, AccessMode.Write);

            // Assert
            allowed.Should().BeTrue();
            await audit.Received(1).LogAsync(expectedEntry, Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Family_with_active_PersonRepresentative_is_allowed()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var familyRepId = Guid.NewGuid();
            var personId = Guid.NewGuid();

            Db.FamilyRepresentatives.Add(new FamilyRepresentative
            {
                Id = familyRepId,
                UserId = userId,
                FirstName = "Ana",
                LastName = "Perez"
            });
            Db.PersonRepresentatives.Add(new PersonRepresentative
            {
                Id = Guid.NewGuid(),
                PersonId = personId,
                RepresentativeId = familyRepId,
                IsActive = true
            });
            await Db.SaveChangesAsync();

            var http = BuildHttpContext(userId, nameof(IdentityRoles.FamilyRepresentative));
            var sut = BuildSut(http, out var audit);

            var expectedEntry = BuildExpectedEntry(
                userId, nameof(IdentityRoles.FamilyRepresentative), personId, AccessAuditValues.Result.Allowed);

            // Act
            var allowed = await sut.CanAccessPersonAsync(personId, AccessMode.Read);

            // Assert
            allowed.Should().BeTrue();
            await audit.Received(1).LogAsync(expectedEntry, Arg.Any<CancellationToken>());
        }

        // ── EntityId optimization path (claim encriptado en JWT) ─────────────
        // Estos tests verifican que cuando el entityId llega en el token (sin join extra),
        // la autorización sigue funcionando correctamente.

        private static IHttpContextService BuildHttpContextWithEntityId(
            Guid userId, string role, Guid entityId, bool isGlobalAdmin = false)
        {
            var svc = Substitute.For<IHttpContextService>();
            svc.GetCurrentUserId().Returns(userId);
            svc.GetCurrentUserRole().Returns(role);
            svc.IsGlobalAdmin().Returns(isGlobalAdmin);
            svc.GetInstitutionIds().Returns(new List<int>());
            svc.GetCurrentEntityId().Returns(entityId);
            svc.GetClientIpAddress().Returns("127.0.0.1");
            svc.GetCorrelationId().Returns("test-correlation");
            return svc;
        }

        [Fact]
        public async Task Professional_withEntityId_can_access_assigned_person()
        {
            // Arrange — professionalId conocido sin necesidad de join por UserId
            var userId         = Guid.NewGuid();
            var professionalId = Guid.NewGuid();
            var personId       = Guid.NewGuid();

            Db.Professionals.Add(new Professional { Id = professionalId, UserId = userId });
            Db.ProfessionalPersons.Add(new ProfessionalPerson
            {
                ProfessionalId = professionalId,
                PersonId       = personId,
                IsActive       = true
            });
            await Db.SaveChangesAsync();

            var http = BuildHttpContextWithEntityId(userId, nameof(IdentityRoles.Professional), professionalId);
            var sut  = BuildSut(http, out _);

            // Act
            var allowed = await sut.CanAccessPersonAsync(personId, AccessMode.Read);

            // Assert
            allowed.Should().BeTrue("el entityId del JWT coincide con el profesional asignado");
        }

        [Fact]
        public async Task Professional_withEntityId_cannot_access_unassigned_person()
        {
            // Arrange — entityId en JWT no tiene vínculo con la persona
            var userId         = Guid.NewGuid();
            var professionalId = Guid.NewGuid();
            var otherPersonId  = Guid.NewGuid();

            Db.Professionals.Add(new Professional { Id = professionalId, UserId = userId });
            // No ProfessionalPerson row
            await Db.SaveChangesAsync();

            var http = BuildHttpContextWithEntityId(userId, nameof(IdentityRoles.Professional), professionalId);
            var sut  = BuildSut(http, out _);

            // Act
            var allowed = await sut.CanAccessPersonAsync(otherPersonId, AccessMode.Read);

            // Assert
            allowed.Should().BeFalse("el profesional no tiene asignada esa persona");
        }

        [Fact]
        public async Task Family_withEntityId_can_access_linked_person()
        {
            // Arrange — familyId conocido desde el JWT
            var userId   = Guid.NewGuid();
            var familyId = Guid.NewGuid();
            var personId = Guid.NewGuid();

            Db.FamilyRepresentatives.Add(new FamilyRepresentative
            {
                Id = familyId, UserId = userId, FirstName = "Ana", LastName = "Perez"
            });
            Db.PersonRepresentatives.Add(new PersonRepresentative
            {
                Id = Guid.NewGuid(), PersonId = personId, RepresentativeId = familyId, IsActive = true
            });
            await Db.SaveChangesAsync();

            var http = BuildHttpContextWithEntityId(userId, nameof(IdentityRoles.FamilyRepresentative), familyId);
            var sut  = BuildSut(http, out _);

            // Act
            var allowed = await sut.CanAccessPersonAsync(personId, AccessMode.Read);

            // Assert
            allowed.Should().BeTrue("el familyId del JWT tiene vínculo activo con la persona");
        }

        [Fact]
        public async Task CanAccessInvitation_NoPerson_ProfessionalOwner_WithEntityId_Allowed()
        {
            // Invitación sin persona: solo el creador puede acceder.
            // El entityId del JWT elimina el query extra a BD.
            var userId         = Guid.NewGuid();
            var professionalId = Guid.NewGuid();

            var invitation = new Domain.Models.Invitation
            {
                Id                       = 1,
                ForPersonId              = null,
                CreatedByProfessionalId  = professionalId,
                Code                     = "ABC123",
                Email                    = "test@example.com",
                ExpiresAt                = DateTime.UtcNow.AddDays(7),
                CreatedByProfessional    = new Professional { Id = professionalId, UserId = userId }
            };
            Db.Invitations.Add(invitation);
            await Db.SaveChangesAsync();

            var http = BuildHttpContextWithEntityId(userId, nameof(IdentityRoles.Professional), professionalId);
            var sut  = BuildSut(http, out _);

            var allowed = await sut.CanAccessInvitationAsync(invitation.Id, AccessMode.Read);

            allowed.Should().BeTrue("el creador de la invitación siempre puede acceder a ella");
        }

        [Fact]
        public async Task CanAccessInvitation_NoPerson_OtherProfessional_WithEntityId_Denied()
        {
            // Invitación sin persona creada por otro profesional → denegado.
            var creatorUserId     = Guid.NewGuid();
            var creatorProfessId  = Guid.NewGuid();
            var otherUserId       = Guid.NewGuid();
            var otherProfessId    = Guid.NewGuid();

            var invitation = new Domain.Models.Invitation
            {
                Id                       = 2,
                ForPersonId              = null,
                CreatedByProfessionalId  = creatorProfessId,
                Code                     = "XYZ789",
                Email                    = "other@example.com",
                ExpiresAt                = DateTime.UtcNow.AddDays(7),
                CreatedByProfessional    = new Professional { Id = creatorProfessId, UserId = creatorUserId }
            };
            Db.Invitations.Add(invitation);
            await Db.SaveChangesAsync();

            // El profesional que consulta es diferente al creador
            var http = BuildHttpContextWithEntityId(otherUserId, nameof(IdentityRoles.Professional), otherProfessId);
            var sut  = BuildSut(http, out _);

            var allowed = await sut.CanAccessInvitationAsync(invitation.Id, AccessMode.Read);

            allowed.Should().BeFalse("solo el profesional creador puede acceder a la invitación");
        }

        [Fact]
        public async Task CanSuperviseLogin_Professional_WithEntityId_Allowed_WhenLinkExists()
        {
            // El profesional tiene permiso de supervisión de login para esa persona.
            var userId         = Guid.NewGuid();
            var professionalId = Guid.NewGuid();
            var personId       = Guid.NewGuid();

            Db.Professionals.Add(new Professional { Id = professionalId, UserId = userId });
            Db.ProfessionalPersons.Add(new ProfessionalPerson
            {
                ProfessionalId    = professionalId,
                PersonId          = personId,
                IsActive          = true,
                CanSuperviseLogin = true
            });
            await Db.SaveChangesAsync();

            var http = BuildHttpContextWithEntityId(userId, nameof(IdentityRoles.Professional), professionalId);
            var sut  = BuildSut(http, out _);

            var allowed = await sut.CanSuperviseLoginAsync(personId);

            allowed.Should().BeTrue("el vínculo tiene CanSuperviseLogin = true");
        }
    }
}
