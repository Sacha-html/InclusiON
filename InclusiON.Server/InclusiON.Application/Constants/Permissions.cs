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
        public const string IsActiveClaimType  = "isActive";
        /// <summary>
        /// ID del entity de dominio del usuario (professionalId, familyRepresentativeId o personId).
        /// El valor viaja encriptado con AES-256-GCM — opaco para quien lea el JWT en crudo.
        /// </summary>
        public const string EntityIdClaimType   = "eid";

        /// <summary>
        /// Nombre de la política especial para administradores globales.
        /// Usar en [Authorize(Policy = Permissions.GlobalAdmin)] en lugar del literal "global-admin".
        /// </summary>
        public const string GlobalAdmin = "global-admin";

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
            public const string LinkFamily = "professionals:link-family";
            public const string UnlinkFamily = "professionals:unlink-family";
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
            public const string Link = "family:link";
            public const string Unlink = "family:unlink";
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
            public const string Read   = "reports:read";
            public const string Create = "reports:create";
            public const string Submit = "reports:submit";   // Profesional envía borrador al admin
            public const string Approve = "reports:approve"; // Admin aprueba
            public const string Reject  = "reports:reject";  // Admin rechaza
            public const string Export  = "reports:export";  // Descargar PDF
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
        // DIAGNÓSTICOS
        // ═══════════════════════════════════════════════════════════════
        public static class Diagnoses
        {
            public const string Read = "diagnoses:read";
            public const string Create = "diagnoses:create";
            public const string Update = "diagnoses:update";
        }

        // ═══════════════════════════════════════════════════════════════
        // ROADMAP
        // ═══════════════════════════════════════════════════════════════
        public static class Roadmap
        {
            public const string Read   = "roadmap:read";
            public const string Create = "roadmap:create";
            public const string Update = "roadmap:update";
            public const string Delete = "roadmap:delete";
        }

        // ═══════════════════════════════════════════════════════════════
        // INVITACIONES
        // ═══════════════════════════════════════════════════════════════
        public static class Invitations
        {
            public const string Read   = "invitations:read";
            public const string Create = "invitations:create";
        }

        // ═══════════════════════════════════════════════════════════════
        // INSTITUCIONES
        // ═══════════════════════════════════════════════════════════════
        public static class Institutions
        {
            public const string Read   = "institutions:read";
            public const string Create = "institutions:create";
            public const string Update = "institutions:update";
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
            Family.Read, Family.Create, Family.Update, Family.Delete, Family.Link, Family.Unlink,
            Activities.Read, Activities.Create, Activities.Update, Activities.Delete, Activities.Respond,
            Diagnoses.Read, Diagnoses.Create, Diagnoses.Update,
            Reports.Read, Reports.Create, Reports.Submit, Reports.Approve, Reports.Reject, Reports.Export,
            Roadmap.Read, Roadmap.Create, Roadmap.Update, Roadmap.Delete,
            Messages.Read, Messages.Create,
            Invitations.Read, Invitations.Create,
            Institutions.Read, Institutions.Create, Institutions.Update,
            Settings.Read, Settings.Update,
            Audit.Read
        };
    }
}
