using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Reports.Queries;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Reports;
using InclusiON.Shared.Resources;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InclusiON.Application.UseCases.Reports.Handlers
{
    public class GetReportByIdQueryHandler : IQueryHandler<GetReportByIdQuery, ApiResponse<ReportResponse>>
    {
        private readonly IReportsRepository _repository;

        public GetReportByIdQueryHandler(IReportsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<ReportResponse>> HandleAsync(GetReportByIdQuery query, CancellationToken cancellationToken)
        {
            var report = await _repository.GetByIdAsync(query.ReportId, cancellationToken);

            if(report == null)
            {
                return ApiResponse<ReportResponse>.ErrorResult(
                    ErrorCode.ReportNotFound,
                    ErrorMessages.ReportNotFound);
            }

            var response = GetReportByIdQuery.MapToResponse(report);
            return ApiResponse<ReportResponse>.SuccessResult(response);
        }
                
    }
}
