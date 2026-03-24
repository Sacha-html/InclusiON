namespace InclusiON.Application.Constants
{
    /// <summary>
    /// Constantes de permisos del sistema.
    /// Usar estas constantes en [Authorize(Policy = "...")] y en verificaciones de permisos.
    /// </summary>
    public static class Permissions
    {
        public const string ClaimType = "permission";
        public const string GlobalAdminClaimType = "isGlobalAdmin";
        public const string InstitutionIdClaimType = "institutionId";
        public const string IsActiveClaimType = "isActive";

        // ═══════════════════════════════════════════════════════════════
        // USUARIOS
        // ═══════════════════════════════════════════════════════════════
        public static class Users
        {
            public const string Read = "users:read";
            public const string Create = "users:create";
            public const string Update = "users:update";
            public const string Delete = "users:delete";
        }

        // ═══════════════════════════════════════════════════════════════
        // PERSONAS CON DISCAPACIDAD
        // ═══════════════════════════════════════════════════════════════
        public static class Persons
        {
            public const string Read = "persons:read";
            public const string Create = "persons:create";
            public const string Update = "persons:update";
            public const string Delete = "persons:delete";
        }

        // ═══════════════════════════════════════════════════════════════
        // PROFESIONALES
        // ═══════════════════════════════════════════════════════════════
        public static class Professionals
        {
            public const string Read = "professionals:read";
            public const string Create = "professionals:create";
            public const string Update = "professionals:update";
            public const string Delete = "professionals:delete";
        }

        // ═══════════════════════════════════════════════════════════════
        // FAMILIARES
        // ═══════════════════════════════════════════════════════════════
        public static class Family
        {
            public const string Read = "family:read";
            public const string Create = "family:create";
            public const string Update = "family:update";
            public const string Delete = "family:delete";
        }

        // ═══════════════════════════════════════════════════════════════
        // ACTIVIDADES
        // ═══════════════════════════════════════════════════════════════
        public static class Activities
        {
            public const string Read = "activities:read";
            public const string Create = "activities:create";
            public const string Update = "activities:update";
            public const string Delete = "activities:delete";
            public const string Respond = "activities:respond";
        }

        // ═══════════════════════════════════════════════════════════════
        // REPORTES
        // ═══════════════════════════════════════════════════════════════
        public static class Reports
        {
            public const string Read = "reports:read";
            public const string Create = "reports:create";
            public const string Export = "reports:export";
        }

        // ═══════════════════════════════════════════════════════════════
        // MENSAJES
        // ═══════════════════════════════════════════════════════════════
        public static class Messages
        {
            public const string Read = "messages:read";
            public const string Create = "messages:create";
        }

        // ═══════════════════════════════════════════════════════════════
        // CONFIGURACIÓN
        // ═══════════════════════════════════════════════════════════════
        public static class Settings
        {
            public const string Read = "settings:read";
            public const string Update = "settings:update";
        }

        // ═══════════════════════════════════════════════════════════════
        // AUDITORÍA
        // ═══════════════════════════════════════════════════════════════
        public static class Audit
        {
            public const string Read = "audit:read";
        }

        /// <summary>
        /// Obtiene todos los permisos disponibles en el sistema.
        /// </summary>
        public static IReadOnlyList<string> GetAll() => new[]
        {
            Users.Read, Users.Create, Users.Update, Users.Delete,
            Persons.Read, Persons.Create, Persons.Update, Persons.Delete,
            Professionals.Read, Professionals.Create, Professionals.Update, Professionals.Delete,
            Family.Read, Family.Create, Family.Update, Family.Delete,
            Activities.Read, Activities.Create, Activities.Update, Activities.Delete, Activities.Respond,
            Reports.Read, Reports.Create, Reports.Export,
            Messages.Read, Messages.Create,
            Settings.Read, Settings.Update,
            Audit.Read
        };
    }
}
