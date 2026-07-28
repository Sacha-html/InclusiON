using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace InclusiON.Data.Converters
{
    public class EncryptedStringConverter : ValueConverter<string?, string?>
    {
        public EncryptedStringConverter() : base(
            v => v == null ? null : EncryptionAccessor.Encrypt(v),
            v => v == null ? null : EncryptionAccessor.Decrypt(v))
        { }
    }
}
