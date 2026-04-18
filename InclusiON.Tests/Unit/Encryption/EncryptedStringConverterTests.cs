using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;
using InclusiON.Data.Converters;
using InclusiON.Infrastructure.Services;

namespace InclusiON.Tests.Unit.Encryption
{
    // Inicializa el EncryptionAccessor estático una vez para todos los tests de la clase.
    public class EncryptedStringConverterTests
    {
        private readonly AesGcmEncryptionService _service;
        private readonly EncryptedStringConverter _sut;

        public EncryptedStringConverterTests()
        {
            _service = new AesGcmEncryptionService(
                new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["EncryptionSettings:Key"] = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA="
                    })
                    .Build());

            EncryptionAccessor.Initialize(_service.Encrypt, _service.Decrypt);
            _sut = new EncryptedStringConverter();
        }

        // ConvertToProvider = lo que EF escribe en la DB (encrypt)
        // ConvertFromProvider = lo que EF lee desde la DB (decrypt)

        [Fact]
        public void ConvertToProvider_EncryptsValue()
        {
            var encrypt = _sut.ConvertToProviderExpression.Compile();
            var result  = encrypt("dato sensible");
            result.Should().StartWith("ENC:");
        }

        [Fact]
        public void ConvertFromProvider_DecryptsValue()
        {
            var encrypt = _sut.ConvertToProviderExpression.Compile();
            var decrypt = _sut.ConvertFromProviderExpression.Compile();
            var plain   = "observación clínica";
            decrypt(encrypt(plain)).Should().Be(plain);
        }

        [Fact]
        public void ConvertFromProvider_PlaintextWithoutPrefix_ReturnsAsIs()
        {
            var decrypt = _sut.ConvertFromProviderExpression.Compile();
            decrypt("texto plano antiguo").Should().Be("texto plano antiguo");
        }

        [Fact]
        public void ConvertToProvider_Null_ReturnsNull()
        {
            var encrypt = _sut.ConvertToProviderExpression.Compile();
            encrypt(null).Should().BeNull();
        }

        [Fact]
        public void ConvertFromProvider_Null_ReturnsNull()
        {
            var decrypt = _sut.ConvertFromProviderExpression.Compile();
            decrypt(null).Should().BeNull();
        }
    }
}
