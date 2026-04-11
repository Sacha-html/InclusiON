using InclusiON.Domain.Models;
using System;

namespace InclusiON.DTOs.Responses.Reports
{
    public class ReportResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; }
        public Guid PersonId { get; set; }
        public string PersonName { get; set; } = string.Empty;
        public Guid ProfessionalId { get; set; }
        public string ProfessionalName { get; set; } = string.Empty;
        public int ReportTypeId { get; set; }
        public string ReportTypeName { get; set; } = string.Empty;
        public DateTime? PeriodStartDate { get; set; }
        public DateTime? PeriodEndDate { get; set; }
        public string? AchievedGoals { get; set; }
        public string? AreasToReinforce { get; set; }
        public string? FutureRecommendations { get; set; }
        public string? NextObjectives { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public static ReportResponse MapToResponse(Report report)
        {
            return new ReportResponse
            {
                Id = report.Id,
                Title = report.Title,
                Content = report.Content,
                ReportDate = report.ReportDate,
                PersonId = report.PersonId,
                PersonName = report.Person != null ? $"{report.Person.FirstName} {report.Person.LastName}" : string.Empty,
                ProfessionalId = report.ProfessionalId,
                ProfessionalName = report.Professional != null ? $"{report.Professional.FirstName} {report.Professional.LastName}" : string.Empty,
                ReportTypeId = report.ReportTypeId,
                ReportTypeName = report.ReportType != null ? report.ReportType.Name : string.Empty,
                PeriodStartDate = report.PeriodStartDate,
                PeriodEndDate = report.PeriodEndDate,
                AchievedGoals = report.AchievedGoals,
                AreasToReinforce = report.AreasToReinforce,
                FutureRecommendations = report.FutureRecommendations,
                NextObjectives = report.NextObjectives,
                IsActive = report.IsActive,
                CreatedAt = report.CreatedAt,
                UpdatedAt = report.UpdatedAt
            };
        }
    }
}
