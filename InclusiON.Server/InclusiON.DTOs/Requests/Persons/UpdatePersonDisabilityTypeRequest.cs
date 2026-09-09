using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Persons
{
    /// <summary>
    /// Tipo de discapacidad evaluado por el profesional principal asignado al alumno.
    /// </summary>
    public class UpdatePersonDisabilityTypeRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "El tipo de discapacidad es requerido")]
        public int DisabilityTypeId { get; set; }
    }
}
