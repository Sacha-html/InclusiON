using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Persons
{
    /// <summary>
    /// Request para crear un alumno junto con su tutor a cargo (nuevo o existente) y asignación opcional de aula.
    /// </summary>
    public class CreatePersonWithTutorRequest : IValidatableObject
    {
        /// <summary>
        /// Datos de registro del alumno.
        /// </summary>
        [Required(ErrorMessage = "Los datos del alumno son obligatorios")]
        public CreatePersonRequest Student { get; set; } = null!;

        /// <summary>
        /// ID de un representante familiar ya existente para vincular explícitamente (ej. hermanos).
        /// Si se informa, se ignoran los campos de alta de tutor nuevo.
        /// </summary>
        public Guid? ExistingTutorId { get; set; }

        /// <summary>
        /// Nombre del tutor (requerido si no se vincula un tutor existente).
        /// </summary>
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre del tutor debe tener entre 2 y 100 caracteres")]
        public string? TutorFirstName { get; set; }

        /// <summary>
        /// Apellido del tutor (requerido si no se vincula un tutor existente).
        /// </summary>
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido del tutor debe tener entre 2 y 100 caracteres")]
        public string? TutorLastName { get; set; }

        /// <summary>
        /// Email del tutor (requerido si no se vincula un tutor existente).
        /// </summary>
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        [StringLength(100, ErrorMessage = "El email del tutor no puede superar los 100 caracteres")]
        public string? TutorEmail { get; set; }

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
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El teléfono solo puede contener números")]
        public string? TutorPhone { get; set; }

        /// <summary>
        /// Relación del tutor con el alumno (padre, madre, tutor legal, etc.).
        /// </summary>
        [Required(ErrorMessage = "El parentesco o relación es obligatorio")]
        [StringLength(50, ErrorMessage = "La relación no puede superar los 50 caracteres")]
        public string TutorRelationship { get; set; } = string.Empty;

        /// <summary>
        /// ID del profesional responsable asignado al alumno (obligatorio).
        /// </summary>
        [Required(ErrorMessage = "El profesional responsable es obligatorio")]
        public Guid ProfessionalId { get; set; }

        /// <summary>
        /// ID del aula a la cual asignar opcionalmente al alumno al crearlo.
        /// </summary>
        public Guid? ClassroomId { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ExistingTutorId.HasValue)
            {
                if (ExistingTutorId.Value == Guid.Empty)
                {
                    yield return new ValidationResult(
                        "Debe seleccionar un tutor existente válido.",
                        [nameof(ExistingTutorId)]);
                }
                yield break;
            }

            if (string.IsNullOrWhiteSpace(TutorFirstName))
            {
                yield return new ValidationResult(
                    "El nombre del tutor es obligatorio",
                    [nameof(TutorFirstName)]);
            }

            if (string.IsNullOrWhiteSpace(TutorLastName))
            {
                yield return new ValidationResult(
                    "El apellido del tutor es obligatorio",
                    [nameof(TutorLastName)]);
            }

            if (string.IsNullOrWhiteSpace(TutorEmail))
            {
                yield return new ValidationResult(
                    "El email del tutor es obligatorio",
                    [nameof(TutorEmail)]);
            }
        }
    }
}
