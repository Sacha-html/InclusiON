using System.ComponentModel.DataAnnotations;

namespace InclusiON.DTOs.Requests.Reports
{
    public class RejectReportRequest
    {
        [Required(ErrorMessage = "El motivo del rechazo es obligatorio.")]
        [StringLength(1000, ErrorMessage = "El motivo no puede superar los 1000 caracteres.")]
        public string Comment { get; set; } = string.Empty;
    }
}
