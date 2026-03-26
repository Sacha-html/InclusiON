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
    public class DeactivatePersonCommandHandler : ICommandHandler<DeactivatePersonCommand, ApiResponse<PersonResponse>>
    {
        private readonly IPersonsRepository _repository;
        private readonly IRefreshTokensRepository _refreshTokensRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeactivatePersonCommandHandler> _logger;

        public DeactivatePersonCommandHandler(
            IPersonsRepository repository,
            IRefreshTokensRepository refreshTokensRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeactivatePersonCommandHandler> logger)
        {
            _repository = repository;
            _refreshTokensRepository = refreshTokensRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<ApiResponse<PersonResponse>> HandleAsync(DeactivatePersonCommand command, CancellationToken cancellationToken)
        {
            var person = await _repository.GetByIdAsync(command.PersonId, cancellationToken);

            if (person == null)
            {
                return ApiResponse<PersonResponse>.ErrorResult(
                    ErrorCode.PersonNotFound,
                    ErrorMessages.PersonNotFound);
            }

            // Desactivar usuario
            person.User.IsActive = false;

            // Revocar refresh tokens activos
            await _refreshTokensRepository.RevokeAllUserTokensAsync(
                person.UserId,
                "Persona desactivada",
                cancellationToken);

            await _repository.UpdateAsync(person, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Persona desactivada: {PersonId}, Usuario: {UserId}", command.PersonId, person.UserId);

            var response = PersonMapper.ToResponse(person);
            return ApiResponse<PersonResponse>.SuccessResult(response, SuccessMessages.PersonDeactivated);
        }
    }
}
