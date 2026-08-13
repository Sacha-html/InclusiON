using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Assignments
{
    /// <summary>
    /// Request para crear un aula y asignar múltiples alumnos a ella.
    /// </summary>
    public class CreateClassroomRequest
    {
        /// <summary>
        /// Nombre del aula.
        /// </summary>
        [Required(ErrorMessage = "El nombre del aula es requerido")]
        [StringLength(150, ErrorMessage = "El nombre del aula no puede superar los 150 caracteres")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Lista de IDs de los alumnos a asignar.
        /// </summary>
        public List<Guid>? PersonIds { get; set; } = new();

        /// <summary>
        /// Indica si es el profesional principal de las personas asignadas.
        /// </summary>
        public bool IsPrimaryProfessional { get; set; } = false;

        /// <summary>
        /// Indica si puede supervisar el login de las personas asignadas.
        /// </summary>
        public bool CanSuperviseLogin { get; set; } = false;
    }
}
