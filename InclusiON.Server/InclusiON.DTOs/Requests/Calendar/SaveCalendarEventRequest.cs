using System;

namespace InclusiON.DTOs.Requests.Calendar
{
    public class SaveCalendarEventRequest
    {
        public string? Id { get; set; }
        public string Title { get; set; } = null!;
        public string Type { get; set; } = null!; // Consulta, Tutoría, Clase, Tarea
        public string Date { get; set; } = null!; // YYYY-MM-DD
        public string Time { get; set; } = null!; // HH:MM
        public string? Description { get; set; }
        public string TargetScope { get; set; } = null!; // all, single
        public string? StudentId { get; set; }
    }
}
