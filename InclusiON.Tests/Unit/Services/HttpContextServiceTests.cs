using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Infrastructure.Services;

namespace InclusiON.Tests.Unit.Services
{
    public class HttpContextServiceTests
    {
        // ── Builders ────────────────────────────────────────────────────────

        private static HttpContextService BuildSut(
            ClaimsPrincipal? principal  = null,
            IEncryptionService? encryption = null)
        {
            var accessor = Substitute.For<IHttpContextAccessor>();

            if (principal is not null)
            {
                var httpContext = new DefaultHttpContext { User = principal };
                accessor.HttpContext.Returns(httpContext);
            }

            encryption ??= Substitute.For<IEncryptionService>();
            return new HttpContextService(accessor, encryption);
        }

        private static ClaimsPrincipal BuildPrincipal(params Claim[] claims) =>
            new(new ClaimsIdentity(claims, "Test"));

        // ── GetCurrentUserId ────────────────────────────────────────────────

        [Fact]
        public void GetCurrentUserId_ValidClaim_ReturnsGuid()
        {
            // Arrange
            var userId    = Guid.NewGuid();
            var principal = BuildPrincipal(
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()));
            var sut = BuildSut(principal);

            // Act
            var result = sut.GetCurrentUserId();

            // Assert
            result.Should().Be(userId);
        }

        [Fact]
        public void GetCurrentUserId_NoClaim_ReturnsNull()
        {
            // Arrange
            var sut = BuildSut(BuildPrincipal());

            // Act
            var result = sut.GetCurrentUserId();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetCurrentUserId_NoHttpContext_ReturnsNull()
        {
            // Arrange
            var accessor = Substitute.For<IHttpContextAccessor>();
            accessor.HttpContext.Returns((HttpContext?)null);
            var sut = new HttpContextService(accessor, Substitute.For<IEncryptionService>());

            // Act
            var result = sut.GetCurrentUserId();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetCurrentUserId_InvalidGuidValue_ReturnsNull()
        {
            // Arrange
            var principal = BuildPrincipal(
                new Claim(ClaimTypes.NameIdentifier, "not-a-guid"));
            var sut = BuildSut(principal);

            // Act
            var result = sut.GetCurrentUserId();

            // Assert
            result.Should().BeNull();
        }

        // ── GetCurrentUserRole ───────────────────────────────────────────────

        [Fact]
        public void GetCurrentUserRole_ValidClaim_ReturnsRole()
        {
            // Arrange
            var principal = BuildPrincipal(new Claim(ClaimTypes.Role, "Professional"));
            var sut       = BuildSut(principal);

            // Act
            var result = sut.GetCurrentUserRole();

            // Assert
            result.Should().Be("Professional");
        }

        [Fact]
        public void GetCurrentUserRole_NoClaim_ReturnsNull()
        {
            // Arrange
            var sut = BuildSut(BuildPrincipal());

            // Act
            var result = sut.GetCurrentUserRole();

            // Assert
            result.Should().BeNull();
        }

        // ── IsGlobalAdmin ───────────────────────────────────────────────────

        [Fact]
        public void IsGlobalAdmin_ClaimTrue_ReturnsTrue()
        {
            // Arrange
            var principal = BuildPrincipal(
                new Claim(Permissions.GlobalAdminClaimType, "true"));

            // Act
            var result = BuildSut(principal).IsGlobalAdmin();

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsGlobalAdmin_ClaimFalse_ReturnsFalse()
        {
            // Arrange
            var principal = BuildPrincipal(
                new Claim(Permissions.GlobalAdminClaimType, "false"));

            // Act
            var result = BuildSut(principal).IsGlobalAdmin();

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsGlobalAdmin_NoClaim_ReturnsFalse()
        {
            // Arrange
            var sut = BuildSut(BuildPrincipal());

            // Act
            var result = sut.IsGlobalAdmin();

            // Assert
            result.Should().BeFalse();
        }

        // ── GetInstitutionIds ───────────────────────────────────────────────

        [Fact]
        public void GetInstitutionIds_MultipleIds_ReturnsAll()
        {
            // Arrange
            var principal = BuildPrincipal(
                new Claim(Permissions.InstitutionIdClaimType, "1"),
                new Claim(Permissions.InstitutionIdClaimType, "3"),
                new Claim(Permissions.InstitutionIdClaimType, "7"));
            var sut = BuildSut(principal);

            // Act
            var result = sut.GetInstitutionIds();

            // Assert
            result.Should().BeEquivalentTo([1, 3, 7]);
        }

        [Fact]
        public void GetInstitutionIds_NoClaims_ReturnsEmptyList()
        {
            // Arrange
            var sut = BuildSut(BuildPrincipal());

            // Act
            var result = sut.GetInstitutionIds();

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void GetInstitutionIds_InvalidValue_IsSkipped()
        {
            // Arrange
            var principal = BuildPrincipal(
                new Claim(Permissions.InstitutionIdClaimType, "5"),
                new Claim(Permissions.InstitutionIdClaimType, "invalid"));

            // Act
            var result = BuildSut(principal).GetInstitutionIds();

            // Assert
            result.Should().BeEquivalentTo([5]);
        }

        // ── GetCurrentEntityId ──────────────────────────────────────────────

        [Fact]
        public void GetCurrentEntityId_ValidEncryptedClaim_ReturnsDecryptedGuid()
        {
            // Arrange
            var entityId   = Guid.NewGuid();
            var encrypted  = "ENC:some_encrypted_value";

            var principal  = BuildPrincipal(
                new Claim(Permissions.EntityIdClaimType, encrypted));

            var encryption = Substitute.For<IEncryptionService>();
            encryption.Decrypt(encrypted).Returns(entityId.ToString());

            var sut = BuildSut(principal, encryption);

            // Act
            var result = sut.GetCurrentEntityId();

            // Assert
            result.Should().Be(entityId);
        }

        [Fact]
        public void GetCurrentEntityId_NoClaim_ReturnsNull()
        {
            // Arrange
            var sut = BuildSut(BuildPrincipal());

            // Act
            var result = sut.GetCurrentEntityId();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetCurrentEntityId_DecryptReturnsInvalidGuid_ReturnsNull()
        {
            // Arrange
            var encrypted  = "ENC:garbage";
            var principal  = BuildPrincipal(
                new Claim(Permissions.EntityIdClaimType, encrypted));

            var encryption = Substitute.For<IEncryptionService>();
            encryption.Decrypt(encrypted).Returns("not-a-guid");

            var sut = BuildSut(principal, encryption);

            // Act
            var result = sut.GetCurrentEntityId();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetCurrentEntityId_DecryptThrows_ReturnsNull()
        {
            // Arrange
            // Claim malformado o clave de encriptación incorrecta — no debe explotar
            var encrypted  = "ENC:tampered";
            var principal  = BuildPrincipal(
                new Claim(Permissions.EntityIdClaimType, encrypted));

            var encryption = Substitute.For<IEncryptionService>();
            encryption.Decrypt(encrypted).Throws(new Exception("bad MAC"));

            var sut = BuildSut(principal, encryption);

            // Act
            var result = sut.GetCurrentEntityId();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void GetCurrentEntityId_CallsDecryptWithExactClaimValue()
        {
            // Arrange
            var encrypted  = "ENC:exact_value";
            var entityId   = Guid.NewGuid();
            var principal  = BuildPrincipal(
                new Claim(Permissions.EntityIdClaimType, encrypted));

            var encryption = Substitute.For<IEncryptionService>();
            encryption.Decrypt(encrypted).Returns(entityId.ToString());

            var sut = BuildSut(principal, encryption);

            // Act
            sut.GetCurrentEntityId();

            // Assert
            encryption.Received(1).Decrypt(encrypted);
        }
    }
}
