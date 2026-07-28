namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Valores canonicos para los campos string de <see cref="AccessAudit"/>.
    /// Evita strings magicos en handlers y servicios.
    /// </summary>
    public static class AccessAuditValues
    {
        public static class Action
        {
            public const string Read = "Read";
            public const string Create = "Create";
            public const string Update = "Update";
            public const string Delete = "Delete";
        }

        public static class Result
        {
            public const string Allowed = "Allowed";
            public const string Denied = "Denied";
        }
    }
}
