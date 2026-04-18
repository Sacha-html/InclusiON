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
            var hash = _sut.Hash("1234");
            hash.Should().StartWith("$argon2");
        }

        [Fact]
        public void Hash_SamePin_DifferentHashEachCall()
        {
            var first  = _sut.Hash("1234");
            var second = _sut.Hash("1234");
            first.Should().NotBe(second); // salts aleatorios distintos
        }

        // ── Verify: Argon2id ────────────────────────────────────────────────

        [Fact]
        public void Verify_CorrectPin_ReturnsTrue_NeedsRehashFalse()
        {
            var hash  = _sut.Hash("5678");
            var valid = _sut.Verify(hash, "5678", out var needsRehash);
            valid.Should().BeTrue();
            needsRehash.Should().BeFalse();
        }

        [Fact]
        public void Verify_WrongPin_ReturnsFalse()
        {
            var hash  = _sut.Hash("5678");
            var valid = _sut.Verify(hash, "9999", out var needsRehash);
            valid.Should().BeFalse();
            needsRehash.Should().BeFalse();
        }

        // ── Verify: BCrypt legacy (migración) ───────────────────────────────

        [Fact]
        public void Verify_BCryptHash_CorrectPin_ReturnsTrue_NeedsRehashTrue()
        {
            // workFactor bajo para que el test no tarde
            var bcryptHash = BCrypt.Net.BCrypt.HashPassword("1234", workFactor: 4);
            var valid      = _sut.Verify(bcryptHash, "1234", out var needsRehash);
            valid.Should().BeTrue();
            needsRehash.Should().BeTrue(); // debe migrarse a Argon2id
        }

        [Fact]
        public void Verify_BCryptHash_WrongPin_ReturnsFalse_NeedsRehashFalse()
        {
            var bcryptHash = BCrypt.Net.BCrypt.HashPassword("1234", workFactor: 4);
            var valid      = _sut.Verify(bcryptHash, "0000", out var needsRehash);
            valid.Should().BeFalse();
            needsRehash.Should().BeFalse(); // no hay login exitoso, no se migra
        }

        // ── Verify: edge cases ───────────────────────────────────────────────

        [Fact]
        public void Verify_EmptyHash_ReturnsFalse()
        {
            _sut.Verify(string.Empty, "1234", out _).Should().BeFalse();
        }

        [Fact]
        public void Verify_NullHash_ReturnsFalse()
        {
            _sut.Verify(null!, "1234", out _).Should().BeFalse();
        }
    }
}
