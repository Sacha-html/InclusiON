using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class GetSupervisorCandidatesQueryHandler
        : IQueryHandler<GetSupervisorCandidatesQuery, ApiResponse<PagedResponse<SupervisorCandidateResponse>>>
    {
        private readonly IPersonsRepository _repository;

        public GetSupervisorCandidatesQueryHandler(IPersonsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PagedResponse<SupervisorCandidateResponse>>> HandleAsync(
            GetSupervisorCandidatesQuery query, CancellationToken cancellationToken)
        {
            var professionals   = await _repository.GetSupervisingProfessionalsAsync(query.PersonId, cancellationToken);
            var representatives = await _repository.GetActiveRepresentativesAsync(query.PersonId, cancellationToken);

            var all = professionals.Select(PersonMapper.ToSupervisorCandidate)
                .Concat(representatives.Select(PersonMapper.ToSupervisorCandidate))
                .OrderBy(c => c.FullName)
                .ToList();

            var totalRecords = all.Count;
            var totalPages   = (int)Math.Ceiling(totalRecords / (double)query.PageSize);
            var data         = all.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToList();

            var response = new PagedResponse<SupervisorCandidateResponse>
            {
                Data            = data,
                TotalRecords    = totalRecords,
                TotalPages      = totalPages,
                CurrentPage     = query.Page,
                PageSize        = query.PageSize,
                HasNextPage     = query.Page < totalPages,
                HasPreviousPage = query.Page > 1
            };

            return ApiResponse<PagedResponse<SupervisorCandidateResponse>>.SuccessResult(response);
        }
    }
}
