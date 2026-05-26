namespace InclusiON.DTOs.Responses.Roadmap
{
    /// <summary>
    /// Hoja de ruta personalizada de una persona con discapacidad.
    /// </summary>
    public class RoadmapResponse
    {
        /// <summary>ID del roadmap.</summary>
        public int Id { get; set; }
        public string EncryptedId { get; set; } = string.Empty;

        /// <summary>ID de la persona a la que pertenece el roadmap.</summary>
        public Guid PersonId { get; set; }

        /// <summary>ID del profesional que creo el roadmap.</summary>
        public Guid CreatedByProfessionalId { get; set; }

        /// <summary>Nombre completo del profesional que creo el roadmap.</summary>
        public string CreatedByProfessionalFullName { get; set; } = string.Empty;

        /// <summary>Notas del profesional sobre el plan de trabajo.</summary>
        public string? Notes { get; set; }

        /// <summary>Fecha de creacion del roadmap.</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Ultima actualizacion del roadmap. Null si nunca fue actualizado.</summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Areas de habilidad incluidas, ordenadas por DisplayOrder.</summary>
        public List<RoadmapAreaResponse> Areas { get; set; } = new();
    }
}
