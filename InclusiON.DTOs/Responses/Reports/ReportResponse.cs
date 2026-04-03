using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


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
    }
}
