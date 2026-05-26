using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Persons.Queries;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class GetPersonAccessibilityQueryHandler
        : IQueryHandler<GetPersonAccessibilityQuery, ApiResponse<PersonAccessibilityResponse>>
    {
        private readonly IPersonsRepository _persons;

        public GetPersonAccessibilityQueryHandler(IPersonsRepository persons)
        {
            _persons = persons;
        }

        public async Task<ApiResponse<PersonAccessibilityResponse>> HandleAsync(
            GetPersonAccessibilityQuery query, CancellationToken cancellationToken)
        {
            var person = await _persons.GetByIdAsync(query.PersonId, cancellationToken);
            if (person is null)
                return ApiResponse<PersonAccessibilityResponse>.NotFound("Persona");

            var dto = new PersonAccessibilityResponse
            {
                RequiresLargeFont      = person.RequiresLargeFont,
                RequiresHighContrast   = person.RequiresHighContrast,
                VisualNoiseSensitivity = person.VisualNoiseSensitivity,
                SoundSensitivity       = person.SoundSensitivity,
                ColorBlindnessType     = person.ColorBlindnessType,
            };

            return ApiResponse<PersonAccessibilityResponse>.SuccessResult(dto);
        }
    }
}
