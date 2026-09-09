using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Persons
{
    /// <summary>
    /// Request para actualizar una persona con discapacidad.
    /// </summary>
    public class UpdatePersonRequest
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        public string? FirstName { get; set; }

        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido debe tener entre 2 y 100 caracteres")]
        public string? LastName { get; set; }

        [StringLength(20, MinimumLength = 6, ErrorMessage = "El documento debe tener entre 6 y 20 caracteres")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "El documento solo puede contener letras y números")]
        public string? DocumentNumber { get; set; }

        public DateTime? BirthDate { get; set; }

        public string? PhotoUrl { get; set; }

    }
}
