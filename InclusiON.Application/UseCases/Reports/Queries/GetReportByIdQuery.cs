using InclusiON.Domain.Models;
using InclusiON.DTOs.Responses.Reports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InclusiON.Application.UseCases.Reports.Queries
{
    public record GetReportByIdQuery(int ReportId)
    {
        internal static ReportResponse MapToResponse(Report report)
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
