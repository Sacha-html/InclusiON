using System.Text.Json;
using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Commands;
using InclusiON.Domain.Enums;
using InclusiON.Domain.Models;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Invitations;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Invitations.Handlers
{
    public class CreateInvitationCommandHandler : ICommandHandler<CreateInvitationCommand, ApiResponse<InvitationResponse>>
    {
        private readonly IInvitationsRepository _repository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IIdentityService _identityService;
        private readonly IUniquenessValidationService _uniquenessValidation;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IBackgroundJobRepository _backgroundJobs;
        private readonly ILogger<CreateInvitationCommandHandler> _logger;
        private readonly IDateTimeProvider _dateTime;

        public CreateInvitationCommandHandler(
            IInvitationsRepository repository,
            IProfessionalsRepository professionalsRepository,
            IIdentityService identityService,
            IUniquenessValidationService uniquenessValidation,
            IUnitOfWork unitOfWork,
            IBackgroundJobRepository backgroundJobs,
            ILogger<CreateInvitationCommandHandler> logger,
            IDateTimeProvider dateTime)
        {
            _repository = repository;
            _professionalsRepository = professionalsRepository;
            _identityService = identityService;
            _uniquenessValidation = uniquenessValidation;
            _unitOfWork = unitOfWork;
            _backgroundJobs = backgroundJobs;
            _logger = logger;
            _dateTime = dateTime;
        }

        public async Task<ApiResponse<InvitationResponse>> HandleAsync(CreateInvitationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var professional = await _professionalsRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);
                if (professional == null)
                {
                    return ApiResponse<InvitationResponse>.ErrorResult(
                        ErrorCode.ProfessionalNotFound,
                        ErrorMessages.ProfessionalNotFound);
                }

                if (professional.Status != ProfessionalStatusEnum.Approved)
                {
                    return ApiResponse<InvitationResponse>.ErrorResult(
                        ErrorCode.ProfessionalNotApproved,
                        ErrorMessages.ProfessionalNotApprovedForInvitationCreation);
                }

                var normalizedEmail = UniquenessNormalizer.NormalizeEmail(command.Email) ?? string.Empty;

                // Validar que el email no este ya registrado ni con invitación pendiente activa
                var emailCheck = await _uniquenessValidation.CheckEmailAsync(
                    normalizedEmail,
                    checkPendingInvitations: true,
                    cancellationToken: cancellationToken);
                if (!emailCheck.IsAvailable)
                {
                    return ApiResponse<InvitationResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        emailCheck.Message ?? ErrorMessages.EmailAlreadyRegistered);
                }

                var invitation = new Invitation
                {
                    CreatedByProfessionalId = command.ProfessionalId,
                    ForPersonId = command.PersonId,
                    Code = Guid.NewGuid().ToString(),
                    Email = normalizedEmail,
                    FirstName = command.FirstName?.Trim(),
                    LastName = command.LastName?.Trim(),
                    Relationship = command.Relationship,
                    ExpiresAt = _dateTime.UtcNow.AddDays(7),
                    IsUsed = false
                };

                await _repository.CreateAsync(invitation, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Invitacion creada: {InvitationId}, Email: {Email}", invitation.Id, invitation.Email);

                // Encolar email de invitación (retry automático vía job queue)
                if (!string.IsNullOrEmpty(command.BaseUrl))
                {
                    var inviteUrl = $"{command.BaseUrl.TrimEnd('/')}/#/invite/{invitation.Code}";
                    await _backgroundJobs.CreateAsync(
                        JobTypes.Email,
                        JsonSerializer.Serialize(new EmailPayload
                        {
                            To           = invitation.Email,
                            Subject      = "InclusiON - Invitacion para registro familiar",
                            TemplateName = "invitation",
                            Replacements = new Dictionary<string, string?>
                            {
                                ["RecipientName"] = !string.IsNullOrEmpty(invitation.FirstName) ? invitation.FirstName : "Familiar",
                                ["InviteUrl"]     = inviteUrl,
                                ["ExpiresAt"]     = invitation.ExpiresAt.ToString("dd/MM/yyyy HH:mm"),
                                ["PersonName"]    = invitation.ForPerson != null
                                    ? $"{invitation.ForPerson.FirstName} {invitation.ForPerson.LastName}".Trim()
                                    : null,
                                ["Relationship"]  = invitation.Relationship,
                                ["Year"]          = _dateTime.UtcNow.Year.ToString()
                            }
                        }),
                        maxRetries: 2,
                        cancellationToken: cancellationToken);
                }

                var response = InvitationResponse.MapToResponse(invitation);
                return ApiResponse<InvitationResponse>.SuccessResult(response, SuccessMessages.InvitationCreated);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear invitacion para {Email}", command.Email);
                return ApiResponse<InvitationResponse>.ErrorResult(
                    ErrorCode.InternalError,
                    ErrorMessages.InternalErrorCreateInvitation);
            }
        }

    }
}
