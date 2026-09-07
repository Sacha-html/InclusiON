using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Mappers;
using InclusiON.Application.UseCases.Persons.Commands;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Persons;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Persons.Handlers
{
    public class ReactivatePersonCommandHandler : ICommandHandler<ReactivatePersonCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReactivatePersonCommandHandler> _logger;

        public ReactivatePersonCommandHandler(
            IPersonsRepository repository,
            IUnitOfWork unitOfWork,
            ILogger<ReactivatePersonCommandHandler> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(ReactivatePersonCommand command, CancellationToken cancellationToken)
        {
            var person = await _repository.GetByIdAsync(command.PersonId, cancellationToken);

            if (person == null)
            {
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.PersonNotFound,
                    ErrorMessages.PersonNotFound);
            }

            if (person.User.IsActive)
            {
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.BusinessRuleViolation,
                    "El alumno ya se encuentra activo");
            }

            // Reactivar usuario asociado a la persona
            person.User.IsActive = true;

            await _repository.UpdateAsync(person, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Persona reactivada: {PersonId}, Usuario: {UserId}", command.PersonId, person.UserId);

            var response = PersonMapper.ToResponse(person);
            return ApiResponse<PersonResponse>.SuccessResult(response, "Alumno reactivado exitosamente");
        }
    }
}
