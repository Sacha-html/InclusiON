using System;
using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Calendar
{
    public class CalendarEventResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD
        public string Time { get; set; } = string.Empty; // HH:MM
        public string? Description { get; set; }
        public Guid? StudentId { get; set; }
        public string? StudentName { get; set; }
        public Guid CreatedByProfessionalId { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string TargetScope { get; set; } = string.Empty; // all, single
        public DateTime CreatedAt { get; set; }

        public static CalendarEventResponse From(CalendarEvent c) => new()
        {
            Id                      = c.Id,
            Title                   = c.Title,
            Type                    = c.Type,
            Date                    = c.Date.ToString("yyyy-MM-dd"),
            Time                    = c.Time,
            Description             = c.Description,
            StudentId               = c.StudentId,
            StudentName             = c.StudentName,
            CreatedByProfessionalId = c.CreatedByProfessionalId,
            CreatedBy               = c.CreatedByProfessional != null
                ? $"{c.CreatedByProfessional.FirstName} {c.CreatedByProfessional.LastName}".Trim()
                : string.Empty,
            TargetScope             = c.TargetScope,
            CreatedAt               = c.CreatedAt
        };
    }
}
