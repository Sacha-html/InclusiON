using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class UpdatePersonDisabilityTypeCommandHandler
        : ICommandHandler<UpdatePersonDisabilityTypeCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _persons;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePersonDisabilityTypeCommandHandler(
            IPersonsRepository persons,
            IUnitOfWork unitOfWork)
        {
            _persons = persons;
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(
            UpdatePersonDisabilityTypeCommand command, CancellationToken cancellationToken)
        {
            var person = await _persons.GetByIdAsync(command.PersonId, cancellationToken);
            if (person is null)
                return ApiResponse<PersonResponse>.NotFound("Persona");

            person.DisabilityTypeId = command.DisabilityTypeId;

            await _persons.UpdateAsync(person, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updatedPerson = await _persons.GetByIdAsync(command.PersonId, cancellationToken);
            if (updatedPerson is null)
                return ApiResponse<PersonResponse>.NotFound("Persona");

            return ApiResponse<PersonResponse>.SuccessResult(
                PersonMapper.ToResponse(updatedPerson), "Tipo de discapacidad actualizado.");
        }
    }
}
