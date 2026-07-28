using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Persons
{
    /// <summary>
    /// Request para actualizar el metodo de login de una persona con discapacidad.
    /// </summary>
    public class UpdateLoginMethodRequest
    {
        /// <summary>
        /// ID del metodo de login a asignar (1=STANDARD, 2=PIN, 3=ASSISTED).
        /// </summary>
        [Required(ErrorMessage = "El metodo de login es requerido")]
        [Range(1, 3, ErrorMessage = "Metodo de login invalido")]
        public int LoginMethodId { get; set; }

        /// <summary>
        /// PIN de 4 digitos (requerido si LoginMethodId = 2).
        /// </summary>
        [StringLength(6, MinimumLength = 4, ErrorMessage = "El PIN debe tener entre 4 y 6 digitos")]
        [RegularExpression(@"^\d{4,6}$", ErrorMessage = "El PIN solo debe contener numeros")]
        public string? Pin { get; set; }

        /// <summary>
        /// ID del usuario supervisor (requerido si LoginMethodId = 3).
        /// </summary>
        public Guid? SupervisorUserId { get; set; }
    }
}
