using InclusiON.Domain.Models;

namespace InclusiON.DTOs.Responses.Reports
{
    public class ReportsListItemReponse
    {
        public int Id {  get; set; }
        public string Title {  get; set; } = string.Empty;
        public string Content {  get; set; } = string.Empty;
        public DateTime ReportDate {  get; set; }
        public Guid PersonId { get; set; }
        public string? PersonName { get; set; }
        public Guid ProfessionalId { get; set; }
        public string? ProfessionalName { get; set; }
        public int ReportTypeId { get; set; }
        public string? ReportTypeName { get; set; }
        public string? AchievedGoals { get; set; }
        public string? AreasToReinforce { get; set; }
        public string? FutureRecommendations { get; set; }
        public string? NextObjectives { get; set; }
        public bool IsActive { get; set; }

        public static ReportsListItemReponse MapToResponse(Report r)
        {
            return new ReportsListItemReponse
            {
                Id = r.Id,
                Title = r.Title,
                Content = r.Content,
                ReportDate = r.ReportDate,
                PersonId = r.PersonId,
                PersonName = r.Person != null ? $"{r.Person.FirstName} {r.Person.LastName}".Trim() : null,
                ProfessionalId = r.ProfessionalId,
                ProfessionalName = r.Professional != null ? $"{r.Professional.FirstName} {r.Professional.LastName}".Trim() : null,
                ReportTypeId = r.ReportTypeId,
                ReportTypeName = r.ReportType?.Name,
                AchievedGoals = r.AchievedGoals,
                AreasToReinforce = r.AreasToReinforce,
                FutureRecommendations = r.FutureRecommendations,
                NextObjectives = r.NextObjectives,
                IsActive = r.IsActive
            };
        }
    }
}
