using System;

namespace InclusiON.Domain.Models
{
    public class CalendarEvent
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string Type { get; set; } = null!; // Consulta, Tutoría, Clase, Tarea
        public DateTime Date { get; set; }
        public string Time { get; set; } = null!; // HH:MM
        public string? Description { get; set; }
        public Guid? StudentId { get; set; }
        public string? StudentName { get; set; }
        public Guid CreatedByProfessionalId { get; set; }
        public string TargetScope { get; set; } = null!; // all, single
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public virtual PersonWithDisability? Student { get; set; }
        public virtual Professional CreatedByProfessional { get; set; } = null!;

        public CalendarEvent()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.UtcNow;
            IsActive = true;
        }
    }
}
