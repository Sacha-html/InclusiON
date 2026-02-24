using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class GetPersonsQueryHandler : IQueryHandler<GetPersonsQuery, ApiResponse<PagedResponse<PersonListItemResponse>>>
    {
        private readonly IPersonsRepository _repository;
        private readonly ILogger<GetPersonsQueryHandler> _logger;

        public GetPersonsQueryHandler(
            IPersonsRepository repository,
            ILogger<GetPersonsQueryHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResponse<PersonListItemResponse>>> HandleAsync(
            GetPersonsQuery query,
            CancellationToken cancellationToken)
        {
            try
            {
                var skip = (query.Page - 1) * query.PageSize;

                var (items, totalCount) = await _repository.GetPagedAsync(
                    skip,
                    query.PageSize,
                    query.Search,
                    query.DisabilityTypeId,
                    query.AutonomyLevelId,
                    query.IsActive,
                    query.SortBy,
                    query.SortDirection,
                    cancellationToken);

                var totalPages = (int)Math.Ceiling(totalCount / (double)query.PageSize);

                var response = new PagedResponse<PersonListItemResponse>
                {
                    Data = items.Select(p => new PersonListItemResponse
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        DocumentNumber = p.DocumentNumber,
                        BirthDate = p.BirthDate,
                        PhotoUrl = p.PhotoUrl,
                        AvatarColor = p.AvatarColor,
                        DisabilityTypeId = p.DisabilityTypeId,
                        DisabilityTypeName = p.DisabilityType?.Name,
                        AutonomyLevelId = p.AutonomyLevelId,
                        AutonomyLevelName = p.AutonomyLevel?.Name,
                        LoginMethodName = p.LoginMethod?.Name,
                        IsActive = p.User?.IsActive ?? false
                    }).ToList(),
                    TotalRecords = totalCount,
                    TotalPages = totalPages,
                    CurrentPage = query.Page,
                    PageSize = query.PageSize,
                    HasNextPage = query.Page < totalPages,
                    HasPreviousPage = query.Page > 1
                };

                return ApiResponse<PagedResponse<PersonListItemResponse>>.SuccessResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar personas");
                return ApiResponse<PagedResponse<PersonListItemResponse>>.ErrorResult(
                    ErrorCode.InternalError,
                    ErrorMessages.InternalErrorListPersons);
            }
        }
    }
}
