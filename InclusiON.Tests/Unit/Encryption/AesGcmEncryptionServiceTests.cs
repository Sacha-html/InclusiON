using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;
using InclusiON.Infrastructure.Services;

namespace InclusiON.Tests.Unit.Encryption
{
    public class AesGcmEncryptionServiceTests
    {
        // 32 ceros en base64 — clave de dev válida
        private const string ValidKey = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA=";

        private static AesGcmEncryptionService BuildSut(string? key = null) =>
            new(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["EncryptionSettings:Key"] = key ?? ValidKey
                })
                .Build());

        // ── Encrypt ─────────────────────────────────────────────────────────

        [Fact]
        public void Encrypt_Returns_EncPrefix()
        {
            // Arrange
            var sut = BuildSut();

            // Act
            var result = sut.Encrypt("dato sensible");

            // Assert
            result.Should().StartWith("ENC:");
        }

        [Fact]
        public void Encrypt_SamePlaintext_DifferentCiphertextEachCall()
        {
            // Arrange
            var sut = BuildSut();

            // Act
            var first  = sut.Encrypt("mismo texto");
            var second = sut.Encrypt("mismo texto");

            // Assert
            first.Should().NotBe(second); // nonces aleatorios distintos
        }

        // ── Decrypt ─────────────────────────────────────────────────────────

        [Fact]
        public void Encrypt_Then_Decrypt_ReturnsOriginal()
        {
            // Arrange
            var sut       = BuildSut();
            var plaintext = "diagnóstico TEA moderado";
            var encrypted = sut.Encrypt(plaintext);

            // Act
            var result = sut.Decrypt(encrypted);

            // Assert
            result.Should().Be(plaintext);
        }

        [Fact]
        public void Decrypt_PlaintextWithoutPrefix_ReturnsAsIs()
        {
            // Arrange
            var sut   = BuildSut();
            var plain = "texto sin cifrar";

            // Act
            var result = sut.Decrypt(plain);

            // Assert
            result.Should().Be(plain);
        }

        [Fact]
        public void Decrypt_EmptyString_ReturnsEmptyString()
        {
            // Arrange
            var sut = BuildSut();

            // Act
            var result = sut.Decrypt(string.Empty);

            // Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public void Decrypt_Unicode_RoundtripPreservesContent()
        {
            // Arrange
            var sut       = BuildSut();
            var plaintext = "Observación: niño con TEA, nivel 2. ¡Avances notables!";

            // Act
            var result = sut.Decrypt(sut.Encrypt(plaintext));

            // Assert
            result.Should().Be(plaintext);
        }

        // ── Constructor ─────────────────────────────────────────────────────

        [Fact]
        public void Constructor_MissingKey_Throws()
        {
            // Arrange
            var config = new ConfigurationBuilder().Build(); // sin clave
            var act    = () => new AesGcmEncryptionService(config);

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*EncryptionSettings:Key*");
        }

        [Fact]
        public void Constructor_KeyNot32Bytes_Throws()
        {
            // Arrange
            // 16 bytes en base64 = clave AES-128, no AES-256
            var shortKey = Convert.ToBase64String(new byte[16]);
            var act      = () => BuildSut(shortKey);

            // Assert
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*32-byte*");
        }
    }
}
