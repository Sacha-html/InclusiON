using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Reports;

namespace InclusiON.Application.UseCases.Reports.Handlers
{
    public class GetReportsQueryHandler : IQueryHandler<GetReportsQuery, ApiResponse<PagedResponse<ReportsListItemReponse>>>
    {
        private readonly IReportsRepository _repository;

        public GetReportsQueryHandler(IReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResponse<ReportsListItemReponse>>> HandleAsync(
            GetReportsQuery query,
            CancellationToken cancellationToken)
        {
            var pagedResult = await _repository.GetPagedAsync(
                query.Page,
                query.PageSize,
                query.Search,
                query.PersonId,
                query.ProfessionalId,
                query.ReportTypeId,
                query.IsActive,
                query.SortBy,
                query.SortDirection,
                query.InstitutionIds,
                cancellationToken);

            var response = new PagedResponse<ReportsListItemReponse>
            {
                Data = pagedResult.Data.Select(r => new ReportsListItemReponse
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
                }).ToList(),
                TotalRecords = pagedResult.TotalRecords,
                TotalPages = pagedResult.TotalPages,
                CurrentPage = pagedResult.CurrentPage,
                PageSize = pagedResult.PageSize,
                HasNextPage = pagedResult.HasNextPage,
                HasPreviousPage = pagedResult.HasPreviousPage
            };

            return ApiResponse<PagedResponse<ReportsListItemReponse>>.SuccessResult(response);
        }
    }
}