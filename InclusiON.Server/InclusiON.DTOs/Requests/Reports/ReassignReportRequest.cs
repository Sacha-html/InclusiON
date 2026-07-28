using System;
using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Reports
{
    public class ReassignReportRequest
    {
        [Required(ErrorMessage = "El ID del nuevo profesional es obligatorio.")]
        public Guid NewProfessionalId { get; set; }
    }
}
