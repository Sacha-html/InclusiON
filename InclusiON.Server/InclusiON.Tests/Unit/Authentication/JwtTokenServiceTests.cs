using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSubstitute;
using Xunit;
using InclusiON.Application.Constants;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.DTOs.Common;
using InclusiON.Infrastructure.Authentication;
using InclusiON.Infrastructure.Configuration;

namespace InclusiON.Tests.Unit.Authentication
{
    public class JwtTokenServiceTests
    {
        private const string ValidSecret  = "SuperSecretKeyForTestingAtLeast32Characters!";
        private const string ValidIssuer   = "InclusiON.Api";
        private const string ValidAudience = "InclusiON.Client";

        // ── Builders ────────────────────────────────────────────────────────

        private static JwtTokenService BuildSut(IEncryptionService? encryption = null)
        {
            var settings = Options.Create(new JwtSettings
            {
                Secret          = ValidSecret,
                Issuer          = ValidIssuer,
                Audience        = ValidAudience,
                ExpirationHours = 1
            });

            encryption ??= Substitute.For<IEncryptionService>();
            return new JwtTokenService(settings, encryption);
        }

        private static TokenUserData BuildUserData(
            string role       = "Professional",
            Guid?  entityId   = null,
            bool   isActive   = true,
            List<string>? permissions  = null,
            List<int>?    institutionIds = null) => new()
        {
            Id             = Guid.NewGuid(),
            Name           = "Test User",
            Email          = "test@inclusion.app",
            Role           = role,
            IsActive       = isActive,
            EntityId       = entityId,
            Permissions    = permissions    ?? [],
            InstitutionIds = institutionIds ?? []
        };

        private static JwtSecurityToken Decode(string token) =>
            new JwtSecurityTokenHandler().ReadJwtToken(token);

        // ── GenerateAccessToken — claims base ───────────────────────────────

        [Fact]
        public void GenerateAccessToken_ValidData_ContainsStandardClaims()
        {
            // Arrange
            var sut  = BuildSut();
            var data = BuildUserData();

            // Act
            var raw  = sut.GenerateAccessToken(data);
            var jwt  = Decode(raw);

            // Assert
            // JwtSecurityTokenHandler mapea ClaimTypes.NameIdentifier → "nameid" (short claim type)
            jwt.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == data.Id.ToString());
            jwt.Issuer.Should().Be(ValidIssuer);
            jwt.Audiences.Should().Contain(ValidAudience);
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti);
            jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Iat);
        }

        [Fact]
        public void GenerateAccessToken_ValidData_ExpiresInConfiguredHours()
        {
            // Arrange
            var sut  = BuildSut();
            var data = BuildUserData();
            var before = DateTime.UtcNow.AddHours(0.9);
            var after  = DateTime.UtcNow.AddHours(1.1);

            // Act
            var raw = sut.GenerateAccessToken(data);
            var jwt = Decode(raw);

            // Assert
            jwt.ValidTo.Should().BeAfter(before).And.BeBefore(after);
        }

        [Fact]
        public void GenerateAccessToken_IsActiveTrue_EmbeddsIsActiveClaim()
        {
            // Arrange
            var sut  = BuildSut();
            var data = BuildUserData(isActive: true);

            // Act
            var jwt  = Decode(sut.GenerateAccessToken(data));

            // Assert
            jwt.Claims.Should().Contain(c =>
                c.Type == Permissions.IsActiveClaimType && c.Value == "True");
        }

        [Fact]
        public void GenerateAccessToken_WithPermissions_IncludesPermissionClaims()
        {
            // Arrange
            var perms = new List<string> { "persons:read", "reports:create" };
            var sut   = BuildSut();
            var data  = BuildUserData(permissions: perms);

            // Act
            var jwt   = Decode(sut.GenerateAccessToken(data));
            var permClaims = jwt.Claims
                .Where(c => c.Type == Permissions.ClaimType)
                .Select(c => c.Value);

            // Assert
            permClaims.Should().BeEquivalentTo(perms);
        }

        [Fact]
        public void GenerateAccessToken_AdminRole_IncludesInstitutionIdClaims()
        {
            // Arrange
            var ids  = new List<int> { 1, 3 };
            var sut  = BuildSut();
            var data = BuildUserData(role: "Admin", institutionIds: ids);

            // Act
            var jwt  = Decode(sut.GenerateAccessToken(data));
            var instClaims = jwt.Claims
                .Where(c => c.Type == Permissions.InstitutionIdClaimType)
                .Select(c => c.Value);

            // Assert
            instClaims.Should().BeEquivalentTo(["1", "3"]);
        }

        // ── GenerateAccessToken — EntityId encriptado ───────────────────────

        [Fact]
        public void GenerateAccessToken_WithEntityId_EncryptsAndEmbedsEidClaim()
        {
            // Arrange
            var entityId   = Guid.NewGuid();
            var encryption = Substitute.For<IEncryptionService>();
            encryption.Encrypt(entityId.ToString()).Returns("ENC:fake_encrypted");

            var sut  = BuildSut(encryption);
            var data = BuildUserData(entityId: entityId);

            // Act
            var jwt  = Decode(sut.GenerateAccessToken(data));

            // Assert
            encryption.Received(1).Encrypt(entityId.ToString());
            jwt.Claims.Should().Contain(c =>
                c.Type == Permissions.EntityIdClaimType && c.Value == "ENC:fake_encrypted");
        }

        [Fact]
        public void GenerateAccessToken_WithoutEntityId_NoEidClaim()
        {
            // Arrange
            var sut  = BuildSut();
            var data = BuildUserData(entityId: null);

            // Act
            var jwt  = Decode(sut.GenerateAccessToken(data));

            // Assert
            jwt.Claims.Should().NotContain(c => c.Type == Permissions.EntityIdClaimType);
        }

        [Fact]
        public void GenerateAccessToken_WithEntityId_DoesNotLeakPlaintextId()
        {
            // Arrange
            var entityId   = Guid.NewGuid();
            var encryption = Substitute.For<IEncryptionService>();
            encryption.Encrypt(Arg.Any<string>()).Returns("ENC:opaque_value");

            var sut = BuildSut(encryption);

            // Act
            var raw = sut.GenerateAccessToken(BuildUserData(entityId: entityId));

            // Assert
            // El ID en texto plano no debe aparecer en el token raw (incluye header.payload.sig)
            raw.Should().NotContain(entityId.ToString());
        }

        // ── ValidateToken ───────────────────────────────────────────────────

        [Fact]
        public void ValidateToken_ValidToken_ReturnsPrincipalWithSubject()
        {
            // Arrange
            var sut  = BuildSut();
            var data = BuildUserData();
            var raw  = sut.GenerateAccessToken(data);

            // Act
            var principal = sut.ValidateToken(raw);

            // Assert
            principal.Should().NotBeNull();
            principal!.FindFirst(ClaimTypes.NameIdentifier)!.Value
                      .Should().Be(data.Id.ToString());
        }

        [Fact]
        public void ValidateToken_TamperedPayload_ReturnsNull()
        {
            // Arrange
            var sut = BuildSut();
            var raw = sut.GenerateAccessToken(BuildUserData());

            // Reemplazar el payload con basura manteniendo header y firma
            var parts    = raw.Split('.');
            var tampered = $"{parts[0]}.{Convert.ToBase64String(new byte[32])}.{parts[2]}";

            // Act
            var result = sut.ValidateToken(tampered);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_EmptyToken_ReturnsNull()
        {
            // Arrange
            var sut = BuildSut();

            // Act
            var result = sut.ValidateToken(string.Empty);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_TokenSignedWithDifferentSecret_ReturnsNull()
        {
            // Arrange
            var otherSettings = Options.Create(new JwtSettings
            {
                Secret          = "OtherSecretKeyForTestingAtLeast32Characters!",
                Issuer          = ValidIssuer,
                Audience        = ValidAudience,
                ExpirationHours = 1
            });
            var other = new JwtTokenService(otherSettings, Substitute.For<IEncryptionService>());
            var sut   = BuildSut();
            var raw = other.GenerateAccessToken(BuildUserData());

            // Act
            var result = sut.ValidateToken(raw);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_ExpiredToken_ReturnsNull()
        {
            // Arrange
            // Construir un JWT con exp en el pasado usando el handler directamente
            var key = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(ValidSecret));

            var handler    = new JwtSecurityTokenHandler();
            var descriptor = new SecurityTokenDescriptor
            {
                NotBefore          = DateTime.UtcNow.AddHours(-2), // nbf también en el pasado
                Expires            = DateTime.UtcNow.AddHours(-1), // ya vencido
                Issuer             = ValidIssuer,
                Audience           = ValidAudience,
                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            };
            var expiredRaw = handler.WriteToken(handler.CreateToken(descriptor));
            var sut = BuildSut();

            // Act
            var result = sut.ValidateToken(expiredRaw);

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void ValidateToken_AlgorithmNone_ReturnsNull()
        {
            // Arrange
            // Construir manualmente un JWT con alg:none — ataque de confusión de algoritmo
            var payload = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(
                    $"{{\"sub\":\"{Guid.NewGuid()}\",\"exp\":9999999999}}"))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var header   = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes("{\"alg\":\"none\",\"typ\":\"JWT\"}"))
                .TrimEnd('=').Replace('+', '-').Replace('/', '_');
            var noneToken = $"{header}.{payload}.";
            var sut = BuildSut();

            // Act
            var result = sut.ValidateToken(noneToken);

            // Assert
            result.Should().BeNull();
        }

        // ── GenerateRefreshToken ─────────────────────────────────────────────

        [Fact]
        public void GenerateRefreshToken_ReturnsDifferentValuesEachCall()
        {
            // Arrange
            var sut = BuildSut();

            // Act
            var first  = sut.GenerateRefreshToken();
            var second = sut.GenerateRefreshToken();

            // Assert
            first.Should().NotBe(second);
        }

        [Fact]
        public void GenerateRefreshToken_IsBase64Encoded()
        {
            // Arrange
            var sut   = BuildSut();
            var token = sut.GenerateRefreshToken();
            var act   = () => Convert.FromBase64String(token);

            // Assert
            act.Should().NotThrow();
            Convert.FromBase64String(token).Should().HaveCount(64); // 64 bytes
        }

        // ── Constructor — validación de configuración ────────────────────────

        [Fact]
        public void Constructor_EmptySecret_Throws()
        {
            // Arrange
            var settings = Options.Create(new JwtSettings
            {
                Secret = string.Empty, Issuer = ValidIssuer, Audience = ValidAudience
            });
            var act = () => new JwtTokenService(settings, Substitute.For<IEncryptionService>());

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Secret*");
        }

        [Fact]
        public void Constructor_SecretTooShort_Throws()
        {
            // Arrange
            var settings = Options.Create(new JwtSettings
            {
                Secret = "short", Issuer = ValidIssuer, Audience = ValidAudience
            });
            var act = () => new JwtTokenService(settings, Substitute.For<IEncryptionService>());

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*32*");
        }

        [Fact]
        public void Constructor_EmptyIssuer_Throws()
        {
            // Arrange
            var settings = Options.Create(new JwtSettings
            {
                Secret = ValidSecret, Issuer = string.Empty, Audience = ValidAudience
            });
            var act = () => new JwtTokenService(settings, Substitute.For<IEncryptionService>());

            // Assert
            act.Should().Throw<ArgumentException>().WithMessage("*Issuer*");
        }

        [Fact]
        public void Constructor_NullEncryptionService_Throws()
        {
            // Arrange
            var settings = Options.Create(new JwtSettings
            {
                Secret = ValidSecret, Issuer = ValidIssuer, Audience = ValidAudience
            });
            var act = () => new JwtTokenService(settings, null!);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
