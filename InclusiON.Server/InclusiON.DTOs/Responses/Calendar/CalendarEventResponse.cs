using System;
using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Calendar
{
    public class CalendarEventResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty; // YYYY-MM-DD
        public string Time { get; set; } = string.Empty; // HH:MM
        public string? Description { get; set; }
        public string? StudentId { get; set; }
        public string? StudentName { get; set; }
        public string CreatedByProfessionalId { get; set; } = string.Empty;
        public string TargetScope { get; set; } = string.Empty; // all, single
        public DateTime CreatedAt { get; set; }

        public static CalendarEventResponse From(CalendarEvent c) => new()
        {
            Id = c.Id.ToString(),
            Title = c.Title,
            Type = c.Type,
            Date = c.Date.ToString("yyyy-MM-dd"),
            Time = c.Time,
            Description = c.Description,
            StudentId = c.StudentId?.ToString(),
            StudentName = c.StudentName,
            CreatedByProfessionalId = c.CreatedByProfessionalId.ToString(),
            TargetScope = c.TargetScope,
            CreatedAt = c.CreatedAt
        };
    }
}
