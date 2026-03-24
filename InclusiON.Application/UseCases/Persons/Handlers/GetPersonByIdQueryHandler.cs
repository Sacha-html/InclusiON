using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class GetPersonByIdQueryHandler : IQueryHandler<GetPersonByIdQuery, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;

        public GetPersonByIdQueryHandler(IPersonsRepository repository)
        {
            _repository = repository;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(GetPersonByIdQuery query, CancellationToken cancellationToken)
        {
            var person = await _repository.GetByIdAsync(query.PersonId, cancellationToken);

            if (person == null)
            {
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.PersonNotFound,
                    ErrorMessages.PersonNotFound);
            }

            var response = PersonMapper.ToResponse(person);
            return ApiResponse<PersonResponse>.SuccessResult(response);
        }
    }
}
