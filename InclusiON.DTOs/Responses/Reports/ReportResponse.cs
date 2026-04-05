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
        public string? PersonName { get; set; }
        public Guid ProfessionalId { get; set; }
        public int ReportTypeId { get; set; }
        public string? AchievedGoals { get; set; }
        public string? AreasToReinforce { get; set; }
        public string? FutureRecommendations { get; set; }
        public string? NextObjectives { get; set; }
        public bool IsActive { get; set; }

        public static ReportResponse MapToResponse(Report report)
        {
            return new ReportResponse
            {
                Id = report.Id,
                Title = report.Title,
                Content = report.Content,
                ReportDate = report.ReportDate,
                PersonId = report.PersonId,
                ProfessionalId = report.ProfessionalId,
                ReportTypeId = report.ReportTypeId,
                AchievedGoals = report.AchievedGoals,
                AreasToReinforce = report.AreasToReinforce,
                FutureRecommendations = report.FutureRecommendations,
                NextObjectives = report.NextObjectives,
                IsActive = report.IsActive,
            };
        }
    }
}
