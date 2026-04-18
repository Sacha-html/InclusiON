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
            var sut    = BuildSut();
            var result = sut.Encrypt("dato sensible");
            result.Should().StartWith("ENC:");
        }

        [Fact]
        public void Encrypt_SamePlaintext_DifferentCiphertextEachCall()
        {
            var sut    = BuildSut();
            var first  = sut.Encrypt("mismo texto");
            var second = sut.Encrypt("mismo texto");
            first.Should().NotBe(second); // nonces aleatorios distintos
        }

        // ── Decrypt ─────────────────────────────────────────────────────────

        [Fact]
        public void Encrypt_Then_Decrypt_ReturnsOriginal()
        {
            var sut       = BuildSut();
            var plaintext = "diagnóstico TEA moderado";
            var encrypted = sut.Encrypt(plaintext);
            sut.Decrypt(encrypted).Should().Be(plaintext);
        }

        [Fact]
        public void Decrypt_PlaintextWithoutPrefix_ReturnsAsIs()
        {
            var sut   = BuildSut();
            var plain = "texto sin cifrar";
            sut.Decrypt(plain).Should().Be(plain);
        }

        [Fact]
        public void Decrypt_EmptyString_ReturnsEmptyString()
        {
            var sut = BuildSut();
            sut.Decrypt(string.Empty).Should().BeEmpty();
        }

        [Fact]
        public void Decrypt_Unicode_RoundtripPreservesContent()
        {
            var sut       = BuildSut();
            var plaintext = "Observación: niño con TEA, nivel 2. ¡Avances notables!";
            sut.Decrypt(sut.Encrypt(plaintext)).Should().Be(plaintext);
        }

        // ── Constructor ─────────────────────────────────────────────────────

        [Fact]
        public void Constructor_MissingKey_Throws()
        {
            var config = new ConfigurationBuilder().Build(); // sin clave
            var act    = () => new AesGcmEncryptionService(config);
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*EncryptionSettings:Key*");
        }

        [Fact]
        public void Constructor_KeyNot32Bytes_Throws()
        {
            // 16 bytes en base64 = clave AES-128, no AES-256
            var shortKey = Convert.ToBase64String(new byte[16]);
            var act      = () => BuildSut(shortKey);
            act.Should().Throw<InvalidOperationException>()
               .WithMessage("*32-byte*");
        }
    }
}
