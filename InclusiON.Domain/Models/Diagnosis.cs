using InclusiON.Domain.Attributes;
using InclusiON.Domain.Models.BaseEntities;

namespace InclusiON.Domain.Models
{
    /// <summary>
    /// Diagnostico o evaluacion inicial de una persona con discapacidad.
    /// Realizado por un profesional, incluye observaciones, capacidades identificadas y recomendaciones.
    /// </summary>
    public class Diagnosis : AuditableBaseEntity
    {
        /// <summary>
        /// Identificador unico del diagnostico.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// ID de la persona diagnosticada.
        /// </summary>
        public Guid PersonId { get; set; }

        /// <summary>
        /// ID del profesional que realiza el diagnostico.
        /// </summary>
        public Guid ProfessionalId { get; set; }

        /// <summary>
        /// Fecha en que se realizo el diagnostico.
        /// </summary>
        public DateTime DiagnosisDate { get; set; }

        /// <summary>
        /// Diagnostico primario o principal.
        /// </summary>
        [Encrypted]
        public string PrimaryDiagnosis { get; set; } = string.Empty;

        /// <summary>
        /// Observaciones iniciales sobre la persona.
        /// </summary>
        [Encrypted]
        public string? InitialObservations { get; set; }

        /// <summary>
        /// Capacidades y fortalezas identificadas.
        /// </summary>
        [Encrypted]
        public string? IdentifiedCapabilities { get; set; }

        /// <summary>
        /// Desafios y areas de mejora identificadas.
        /// </summary>
        [Encrypted]
        public string? IdentifiedChallenges { get; set; }

        /// <summary>
        /// Apoyos requeridos para el desarrollo.
        /// </summary>
        [Encrypted]
        public string? RequiredSupports { get; set; }

        /// <summary>
        /// Objetivos pedagogicos propuestos.
        /// </summary>
        [Encrypted]
        public string? PedagogicalObjectives { get; set; }

        /// <summary>
        /// Estrategias de intervencion recomendadas.
        /// </summary>
        [Encrypted]
        public string? RecommendedStrategies { get; set; }

        /// <summary>
        /// Persona diagnosticada.
        /// </summary>
        public virtual PersonWithDisability Person { get; set; } = null!;

        /// <summary>
        /// Profesional que realizo el diagnostico.
        /// </summary>
        public virtual Professional Professional { get; set; } = null!;
    }
}
