using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Persons
{
    /// <summary>
    /// Request para crear un alumno junto con su tutor a cargo y asignación opcional de aula.
    /// </summary>
    public class CreatePersonWithTutorRequest
    {
        /// <summary>
        /// Datos de registro del alumno.
        /// </summary>
        [Required(ErrorMessage = "Los datos del alumno son obligatorios")]
        public CreatePersonRequest Student { get; set; } = null!;

        /// <summary>
        /// Nombre del tutor.
        /// </summary>
        [Required(ErrorMessage = "El nombre del tutor es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre del tutor debe tener entre 2 y 100 caracteres")]
        public string TutorFirstName { get; set; } = string.Empty;

        /// <summary>
        /// Apellido del tutor.
        /// </summary>
        [Required(ErrorMessage = "El apellido del tutor es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido del tutor debe tener entre 2 y 100 caracteres")]
        public string TutorLastName { get; set; } = string.Empty;

        /// <summary>
        /// Email del tutor (se usará como usuario de acceso).
        /// </summary>
        [Required(ErrorMessage = "El email del tutor es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email del tutor no puede superar los 100 caracteres")]
        public string TutorEmail { get; set; } = string.Empty;

        /// <summary>
        /// Documento del tutor.
        /// </summary>
        [StringLength(20, MinimumLength = 6, ErrorMessage = "El documento del tutor debe tener entre 6 y 20 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "El documento del tutor solo puede contener letras y números")]
        public string? TutorDocumentNumber { get; set; }

        /// <summary>
        /// Teléfono del tutor.
        /// </summary>
        [StringLength(20, ErrorMessage = "El teléfono del tutor no puede superar los 20 caracteres")]
        public string? TutorPhone { get; set; }

        /// <summary>
        /// Relación del tutor con el alumno (padre, madre, tutor legal, etc.).
        /// </summary>
        [Required(ErrorMessage = "El parentesco o relación es obligatorio")]
        [StringLength(50, ErrorMessage = "La relación no puede superar los 50 caracteres")]
        public string TutorRelationship { get; set; } = string.Empty;

        /// <summary>
        /// ID del aula a la cual asignar opcionalmente al alumno al crearlo.
        /// </summary>
        public Guid? ClassroomId { get; set; }
    }
}
