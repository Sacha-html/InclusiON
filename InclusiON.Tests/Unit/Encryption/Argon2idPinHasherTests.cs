using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using InclusiON.Infrastructure.Authentication;

namespace InclusiON.Tests.Unit.Encryption
{
    public class Argon2idPinHasherTests
    {
        private readonly Argon2idPinHasher _sut = new(NullLogger<Argon2idPinHasher>.Instance);

        // ── Hash ────────────────────────────────────────────────────────────

        [Fact]
        public void Hash_ProducesArgon2idFormat()
        {
            // Arrange
            // (sut built in field initializer)

            // Act
            var hash = _sut.Hash("1234");

            // Assert
            hash.Should().StartWith("$argon2");
        }

        [Fact]
        public void Hash_SamePin_DifferentHashEachCall()
        {
            // Arrange
            // (sut built in field initializer)

            // Act
            var first  = _sut.Hash("1234");
            var second = _sut.Hash("1234");

            // Assert
            first.Should().NotBe(second); // salts aleatorios distintos
        }

        // ── Verify: Argon2id ────────────────────────────────────────────────

        [Fact]
        public void Verify_CorrectPin_ReturnsTrue_NeedsRehashFalse()
        {
            // Arrange
            var hash = _sut.Hash("5678");

            // Act
            var valid = _sut.Verify(hash, "5678", out var needsRehash);

            // Assert
            valid.Should().BeTrue();
            needsRehash.Should().BeFalse();
        }

        [Fact]
        public void Verify_WrongPin_ReturnsFalse()
        {
            // Arrange
            var hash = _sut.Hash("5678");

            // Act
            var valid = _sut.Verify(hash, "9999", out var needsRehash);

            // Assert
            valid.Should().BeFalse();
            needsRehash.Should().BeFalse();
        }

        // ── Verify: BCrypt legacy (migración) ───────────────────────────────

        [Fact]
        public void Verify_BCryptHash_CorrectPin_ReturnsTrue_NeedsRehashTrue()
        {
            // Arrange
            // workFactor bajo para que el test no tarde
            var bcryptHash = BCrypt.Net.BCrypt.HashPassword("1234", workFactor: 4);

            // Act
            var valid = _sut.Verify(bcryptHash, "1234", out var needsRehash);

            // Assert
            valid.Should().BeTrue();
            needsRehash.Should().BeTrue(); // debe migrarse a Argon2id
        }

        [Fact]
        public void Verify_BCryptHash_WrongPin_ReturnsFalse_NeedsRehashFalse()
        {
            // Arrange
            var bcryptHash = BCrypt.Net.BCrypt.HashPassword("1234", workFactor: 4);

            // Act
            var valid = _sut.Verify(bcryptHash, "0000", out var needsRehash);

            // Assert
            valid.Should().BeFalse();
            needsRehash.Should().BeFalse(); // no hay login exitoso, no se migra
        }

        // ── Verify: edge cases ───────────────────────────────────────────────

        [Fact]
        public void Verify_EmptyHash_ReturnsFalse()
        {
            // Arrange
            // (sut built in field initializer)

            // Act
            var result = _sut.Verify(string.Empty, "1234", out _);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void Verify_NullHash_ReturnsFalse()
        {
            // Arrange
            // (sut built in field initializer)

            // Act
            var result = _sut.Verify(null!, "1234", out _);

            // Assert
            result.Should().BeFalse();
        }
    }
}
