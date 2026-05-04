using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Family;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class GetPersonRepresentativesQueryHandler
        : IQueryHandler<GetPersonRepresentativesQuery, ApiResponse<List<PersonRepresentativeResponse>>>
    {
        private readonly IFamilyRepository _family;

        public GetPersonRepresentativesQueryHandler(IFamilyRepository family)
        {
            _family = family;
        }

        public async Task<ApiResponse<List<PersonRepresentativeResponse>>> HandleAsync(
            GetPersonRepresentativesQuery query, CancellationToken cancellationToken)
        {
            var representatives = await _family.GetPersonRepresentativesByPersonIdAsync(
                query.PersonId, cancellationToken);

            var response = representatives.Select(pr => new PersonRepresentativeResponse
            {
                PersonId               = pr.PersonId,
                RepresentativeId       = pr.RepresentativeId,
                RepresentativeFullName = $"{pr.Representative.FirstName} {pr.Representative.LastName}",
                Relationship           = pr.Relationship,
                IsPrimary              = pr.IsPrimary,
                IsActive               = pr.IsActive,
                CreatedAt              = pr.CreatedAt,
                UpdatedAt              = pr.UpdatedAt,
                EndedAt                = pr.EndedAt,
                UnlinkObservation      = pr.UnlinkObservation
            }).ToList();

            return ApiResponse<List<PersonRepresentativeResponse>>.SuccessResult(response);
        }
    }
}
