using Microsoft.AspNetCore.Identity;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Usuario del sistema. Extiende IdentityUser para autenticacion.
    /// Puede tener uno de tres perfiles: Professional, PersonWithDisability o FamilyRepresentative.
    /// </summary>
    public class User : IdentityUser<Guid>
    {
        /// <summary>
        /// Nombre del usuario.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Apellido del usuario.
        /// </summary>
        public string? Surname { get; set; }

        /// <summary>
        /// Fecha de creacion del usuario.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Indica si el usuario esta activo en el sistema.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Fecha del ultimo inicio de sesion.
        /// </summary>
        public DateTime? LastLoginDate { get; set; }

        /// <summary>
        /// Direccion IP del ultimo inicio de sesion.
        /// </summary>
        public string? LastLoginIpAddress { get; set; }

        /// <summary>
        /// User Agent del navegador en el ultimo inicio de sesion.
        /// </summary>
        public string? LastLoginUserAgent { get; set; }

        /// <summary>
        /// Indica si el usuario debe cambiar su contraseña en el proximo inicio de sesion.
        /// Usado para contraseñs temporales.
        /// </summary>
        public bool MustChangePassword { get; set; }

        /// <summary>
        /// Tokens de refresco para mantener sesiones activas.
        /// </summary>
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }

        /// <summary>
        /// Perfil de profesional asociado (relacion 1:1 opcional).
        /// </summary>
        public virtual Professional? Professional { get; set; }

        /// <summary>
        /// Perfil de persona con discapacidad asociado (relacion 1:1 opcional).
        /// </summary>
        public virtual PersonWithDisability? PersonWithDisability { get; set; }

        /// <summary>
        /// Perfil de representante familiar asociado (relacion 1:1 opcional).
        /// </summary>
        public virtual FamilyRepresentative? FamilyRepresentative { get; set; }

        /// <summary>
        /// Mensajes enviados por el usuario.
        /// </summary>
        public virtual ICollection<Message> SentMessages { get; set; }

        /// <summary>
        /// Mensajes recibidos por el usuario.
        /// </summary>
        public virtual ICollection<Message> ReceivedMessages { get; set; }

        /// <summary>
        /// Registros de auditoria de acceso realizados por el usuario.
        /// </summary>
        public virtual ICollection<AccessAudit> AccessAudits { get; set; }

        /// <summary>
        /// Dispositivos de confianza registrados para login sin credenciales.
        /// </summary>
        public virtual ICollection<TrustedDevice> TrustedDevices { get; set; }

        /// <summary>
        /// Instituciones asignadas a este usuario administrador.
        /// </summary>
        public virtual ICollection<AdminInstitution> AdminInstitutions { get; set; }

        public User()
        {
            Id = Guid.NewGuid();
            RefreshTokens = new HashSet<RefreshToken>();
            SentMessages = new HashSet<Message>();
            ReceivedMessages = new HashSet<Message>();
            AccessAudits = new HashSet<AccessAudit>();
            TrustedDevices = new HashSet<TrustedDevice>();
            AdminInstitutions = new HashSet<AdminInstitution>();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
            MustChangePassword = false;
        }
    }
}
