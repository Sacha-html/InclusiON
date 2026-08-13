using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Assignments
{
    /// <summary>
    /// Request para renombrar un aula existente.
    /// </summary>
    public class UpdateClassroomRequest
    {
        /// <summary>
        /// Nuevo nombre del aula.
        /// </summary>
        [Required(ErrorMessage = "El nombre del aula es requerido")]
        [StringLength(150, ErrorMessage = "El nombre del aula no puede superar los 150 caracteres")]
        public string Name { get; set; } = string.Empty;
    }
}
