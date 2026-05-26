using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Constants;
using InclusiON.Application.Helpers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Professionals.Commands;
using InclusiON.Application.UseCases.Professionals.Queries;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Professionals;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Professionals.Handlers
{
    public class CreateProfessionalCommandHandler : ICommandHandler<CreateProfessionalCommand, ApiResponse<ProfessionalResponse>>
    {
        private readonly IProfessionalsRepository _repository;
        private readonly IIdentityService _identityService;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateProfessionalCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public CreateProfessionalCommandHandler(
            IProfessionalsRepository repository,
            IIdentityService identityService,
            IBackgroundJobRepository backgroundJobs,
            IUnitOfWork unitOfWork,
            ILogger<CreateProfessionalCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _identityService = identityService;
            _backgroundJobs = backgroundJobs;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<ProfessionalResponse>> HandleAsync(CreateProfessionalCommand command, CancellationToken cancellationToken)
        {
            try
            {
                // Validar documento unico
                if (!string.IsNullOrWhiteSpace(command.DocumentNumber))
                {
                    var documentExists = await _repository.ExistsDocumentAsync(command.DocumentNumber, null, cancellationToken);
                    if (documentExists)
                    {
                        return ApiResponse<ProfessionalResponse>.Conflict(
                            ErrorCode.DocumentAlreadyExists,
                            ErrorMessages.DocumentAlreadyExists);
                    }
                }

                // Validar email unico
                var existingUser = await _identityService.FindByEmailAsync(command.Email);
                if (existingUser != null)
                {
                    return ApiResponse<ProfessionalResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        ErrorMessages.EmailAlreadyRegistered);
                }

                // Generar contraseña temporal
                var password = PasswordGenerator.GenerateTemporary();

                // Crear usuario
                var user = new User
                {
                    UserName = command.Email,
                    Email = command.Email,
                    Name = command.FirstName,
                    Surname = command.LastName,
                    IsActive = true,
                    CreatedAt = _dateTime.UtcNow,
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    MustChangePassword = true,
                };

                // Crear profesional
                var professional = new Professional
                {
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    DocumentNumber = command.DocumentNumber,
                    Phone = command.Phone,
                    Specialty = command.Specialty,
                    LicenseNumber = command.LicenseNumber,
                    BirthDate = command.BirthDate,
                    Status = InclusiON.Domain.Enums.ProfessionalStatusEnum.Approved,
                    IsActive = true
                };

                // Crear usuario, asignar rol y profesional en transaccion
                await _unitOfWork.ExecuteInTransactionAsync(async ct =>
                {
                    var (succeeded, errors) = await _identityService.CreateUserAsync(user, password);
                    if (!succeeded)
                    {
                        throw new InvalidOperationException(string.Format(ErrorMessages.UserCreationError, string.Join(", ", errors)));
                    }

                    await _identityService.AddToRoleAsync(user, RoleNames.Professional);

                    professional.UserId = user.Id;

                    if (command.InstitutionIds != null && command.InstitutionIds.Count > 0)
                    {
                        foreach (var instId in command.InstitutionIds)
                        {
                            professional.ProfessionalInstitutions.Add(new ProfessionalInstitution
                            {
                                ProfessionalId = professional.Id,
                                InstitutionId = instId,
                                AssignedAt = _dateTime.UtcNow,
                                IsActive = true
                            });
                        }
                    }

                    await _repository.CreateAsync(professional, ct);
                    await _unitOfWork.SaveChangesAsync(ct);
                }, cancellationToken);

                _logger.LogInformation("Profesional creado: {ProfessionalId}, Usuario: {UserId}", professional.Id, user.Id);

                await _backgroundJobs.CreateAsync(
                    JobTypes.Email,
                    JsonSerializer.Serialize(new EmailPayload
                    {
                        To           = command.Email,
                        Subject      = "Bienvenido a InclusiON — Tu cuenta ha sido creada",
                        TemplateName = "PasswordReset",
                        Replacements = new Dictionary<string, string?>
                        {
                            { "UserName", command.FirstName },
                            { "TemporaryPassword", password },
                            { "Year", _dateTime.UtcNow.Year.ToString() }
                        }
                    }),
                    maxRetries: 2,
                    cancellationToken: cancellationToken);

                var response = ProfessionalResponse.MapToResponse(professional);
                response.TemporaryPassword = password;
                return ApiResponse<ProfessionalResponse>.SuccessResult(response, SuccessMessages.ProfessionalCreated);
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith(ErrorMessages.UserCreationError.Replace("{0}", "")))
            {
                _logger.LogWarning(ex, "Error de validacion al crear profesional");
                return ApiResponse<ProfessionalResponse>.ErrorResult(
                    ErrorCode.ValidationFailed,
                    ex.Message);
            }
        }

    }
}
