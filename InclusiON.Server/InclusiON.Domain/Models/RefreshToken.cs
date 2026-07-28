using System.ComponentModel.DataAnnotations;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Token de refresco para mantener sesiones de usuario.
    /// Permite renovar el token de acceso sin requerir nuevo login.
    /// </summary>
    public class RefreshToken
    {
        /// <summary>
        /// Identificador unico del token.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Valor del token de refresco (hash seguro).
        /// </summary>
        [Required]
        [MaxLength(512)]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// ID del usuario propietario del token.
        /// </summary>
        [Required]
        public Guid UserId { get; set; }

        /// <summary>
        /// Fecha y hora de expiracion del token.
        /// </summary>
        [Required]
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// Fecha y hora de creacion del token.
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha y hora en que fue revocado.
        /// </summary>
        public DateTime? RevokedAt { get; set; }

        /// <summary>
        /// Indica si el token esta activo.
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Direccion IP desde donde se revoco el token.
        /// </summary>
        [MaxLength(45)]
        public string? RevokedByIp { get; set; }

        /// <summary>
        /// Razon de revocacion del token.
        /// </summary>
        public string? RevokedReason { get; set; }

        /// <summary>
        /// Usuario propietario del token.
        /// </summary>
        public virtual User User { get; set; } = null!;

        #region Campos de Auditoria
        /// <summary>
        /// User Agent del navegador que creo el token.
        /// </summary>
        public string? UserAgent { get; set; }

        /// <summary>
        /// Direccion IP desde donde se creo el token.
        /// </summary>
        [MaxLength(45)]
        public string? CreatedByIp { get; set; }
        #endregion
    }
}
