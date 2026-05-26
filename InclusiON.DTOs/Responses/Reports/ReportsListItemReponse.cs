using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Reports
{
    public class ReportsListItemReponse
    {
        public int Id { get; set; }
        public string EncryptedId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public Guid PersonId { get; set; }
        public string? PersonName { get; set; }
        public Guid ProfessionalId { get; set; }
        public string? ProfessionalName { get; set; }
        public int ReportTypeId { get; set; }
        public string? ReportTypeName { get; set; }
        public bool IsActive { get; set; }
        public ReportStatus Status { get; set; }
        public string? AdminComment { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsReadByFamily { get; set; }

        public static ReportsListItemReponse MapToResponse(Report r) => new()
        {
            Id = r.Id,
            Title = r.Title,
            ReportDate = r.ReportDate,
            PersonId = r.PersonId,
            PersonName = r.Person != null ? $"{r.Person.FirstName} {r.Person.LastName}".Trim() : null,
            ProfessionalId = r.ProfessionalId,
            ProfessionalName = r.Professional != null ? $"{r.Professional.FirstName} {r.Professional.LastName}".Trim() : null,
            ReportTypeId = r.ReportTypeId,
            ReportTypeName = r.ReportType?.Name,
            IsActive = r.IsActive,
            Status = r.Status,
            AdminComment = r.AdminComment,
            ApprovedAt = r.ApprovedAt,
            CreatedAt = r.CreatedAt,
            IsReadByFamily = r.IsReadByFamily,
        };
    }
}
