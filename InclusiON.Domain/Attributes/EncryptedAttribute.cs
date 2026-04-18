namespace InclusiON.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class EncryptedAttribute : Attribute { }
}
