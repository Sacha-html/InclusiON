using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Constants;
using InclusiON.Application.Helpers;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.Interfaces.Repositories.Base;
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
        private readonly IUniquenessValidationService _uniquenessValidation;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateProfessionalCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;
        private readonly IReadOnlyRepository<Specialty> _specialties;

        public CreateProfessionalCommandHandler(
            IProfessionalsRepository repository,
            IIdentityService identityService,
            IUniquenessValidationService uniquenessValidation,
            IBackgroundJobRepository backgroundJobs,
            IUnitOfWork unitOfWork,
            ILogger<CreateProfessionalCommandHandler> logger,
            IDateTimeProvider dateTime,
            IReadOnlyRepository<Specialty> specialties)
        {
            _repository = repository;
            _identityService = identityService;
            _uniquenessValidation = uniquenessValidation;
            _backgroundJobs = backgroundJobs;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _dateTime = dateTime;
            _specialties = specialties;
        }

        public async Task<ApiResponse<ProfessionalResponse>> HandleAsync(CreateProfessionalCommand command, CancellationToken cancellationToken)
        {
            try
            {
                if (!command.SpecialtyId.HasValue)
                    return ApiResponse<ProfessionalResponse>.ErrorResult(ErrorCode.ValidationFailed, "La especialidad es obligatoria.");

                var specialty = await _specialties.GetByIdAsync(command.SpecialtyId.Value, cancellationToken);
                if (specialty?.IsActive != true)
                    return ApiResponse<ProfessionalResponse>.ErrorResult(ErrorCode.ValidationFailed, "La especialidad seleccionada no está activa.");

                var normalizedDoc = UniquenessNormalizer.NormalizeDocument(command.DocumentNumber);
                var normalizedEmail = UniquenessNormalizer.NormalizeEmail(command.Email) ?? string.Empty;
                var normalizedLicense = UniquenessNormalizer.NormalizeLicense(command.LicenseNumber);

                // Validar documento unico global
                if (normalizedDoc is not null)
                {
                    var docCheck = await _uniquenessValidation.CheckDocumentAsync(normalizedDoc, null, cancellationToken);
                    if (!docCheck.IsAvailable)
                    {
                        return ApiResponse<ProfessionalResponse>.Conflict(
                            ErrorCode.DocumentAlreadyExists,
                            docCheck.Message ?? ErrorMessages.DocumentAlreadyExists);
                    }
                }

                // Validar email unico global
                var emailCheck = await _uniquenessValidation.CheckEmailAsync(normalizedEmail, cancellationToken: cancellationToken);
                if (!emailCheck.IsAvailable)
                {
                    return ApiResponse<ProfessionalResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        emailCheck.Message ?? ErrorMessages.EmailAlreadyRegistered);
                }

                // Validar matrícula única
                if (normalizedLicense is not null)
                {
                    var licenseCheck = await _uniquenessValidation.CheckLicenseNumberAsync(normalizedLicense, null, cancellationToken);
                    if (!licenseCheck.IsAvailable)
                    {
                        return ApiResponse<ProfessionalResponse>.Conflict(
                            ErrorCode.LicenseNumberAlreadyExists,
                            licenseCheck.Message ?? "Esta matrícula ya se encuentra registrada.");
                    }
                }

                // Generar contraseña temporal
                var password = PasswordGenerator.GenerateTemporary();

                // Crear usuario
                var user = new User
                {
                    UserName = normalizedEmail,
                    Email = normalizedEmail,
                    Name = command.FirstName.Trim(),
                    Surname = command.LastName.Trim(),
                    IsActive = true,
                    CreatedAt = _dateTime.UtcNow,
                    EmailConfirmed = true,
                    LockoutEnabled = true,
                    MustChangePassword = true,
                };

                // Crear profesional
                var professional = new Professional
                {
                    FirstName = command.FirstName.Trim(),
                    LastName = command.LastName.Trim(),
                    DocumentNumber = normalizedDoc,
                    Email = normalizedEmail,
                    Phone = command.Phone?.Trim(),
                    Specialty = specialty.Name,
                    SpecialtyId = command.SpecialtyId,
                    LicenseNumber = normalizedLicense,
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
                        To           = normalizedEmail,
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
