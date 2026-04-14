using Microsoft.Extensions.Logging;
using InclusiON.Application.Interfaces.Common;
using InclusiON.Application.Interfaces.Infrastructure;
using InclusiON.Application.Interfaces.Repositories;
using InclusiON.Application.UseCases.Invitations.Commands;
using InclusiON.Domain.Enums;
using InclusiON.DTOs.Common;
using InclusiON.DTOs.Responses;
using InclusiON.DTOs.Responses.Invitations;
using InclusiON.Domain.Models;
using InclusiON.Shared.Resources;

namespace InclusiON.Application.UseCases.Invitations.Handlers
{
    public class CreateInvitationCommandHandler : ICommandHandler<CreateInvitationCommand, ApiResponse<InvitationResponse>>
    {
        private readonly IInvitationsRepository _repository;
        private readonly IProfessionalsRepository _professionalsRepository;
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly ILogger<CreateInvitationCommandHandler> _logger;

        public CreateInvitationCommandHandler(
            IInvitationsRepository repository,
            IProfessionalsRepository professionalsRepository,
            IIdentityService identityService,
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            ILogger<CreateInvitationCommandHandler> logger)
        {
            _repository = repository;
            _professionalsRepository = professionalsRepository;
            _identityService = identityService;
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _logger = logger;
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

                // Validar que el email no este ya registrado
                var existingUser = await _identityService.FindByEmailAsync(command.Email);
                if (existingUser != null)
                {
                    return ApiResponse<InvitationResponse>.Conflict(
                        ErrorCode.EmailAlreadyExists,
                        ErrorMessages.EmailAlreadyRegistered);
                }

                var invitation = new Invitation
                {
                    CreatedByProfessionalId = command.ProfessionalId,
                    ForPersonId = command.PersonId,
                    Code = Guid.NewGuid().ToString(),
                    Email = command.Email,
                    FirstName = command.FirstName,
                    LastName = command.LastName,
                    Relationship = command.Relationship,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    IsUsed = false
                };

                await _repository.CreateAsync(invitation, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Invitacion creada: {InvitationId}, Email: {Email}", invitation.Id, invitation.Email);

                // Enviar email con el link de invitacion (fire-and-forget, no bloquea)
                if (!string.IsNullOrEmpty(command.BaseUrl))
                {
                    _ = SendInvitationEmailAsync(invitation, command.BaseUrl, cancellationToken);
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

        private async Task SendInvitationEmailAsync(Invitation invitation, string baseUrl, CancellationToken cancellationToken)
        {
            try
            {
                var inviteUrl = $"{baseUrl.TrimEnd('/')}/#/invite/{invitation.Code}";

                var replacements = new Dictionary<string, string?>
                {
                    ["RecipientName"] = !string.IsNullOrEmpty(invitation.FirstName) ? invitation.FirstName : "Familiar",
                    ["InviteUrl"] = inviteUrl,
                    ["ExpiresAt"] = invitation.ExpiresAt.ToString("dd/MM/yyyy HH:mm"),
                    ["PersonName"] = invitation.ForPerson != null
                        ? $"{invitation.ForPerson.FirstName} {invitation.ForPerson.LastName}".Trim()
                        : null,
                    ["Relationship"] = invitation.Relationship,
                    ["Year"] = DateTime.UtcNow.Year.ToString()
                };

                // TODO: Refactorizar usando Microsoft.Extensions.AI / Semantic Kernel Agent Framework
                // para orquestar notificaciones de forma inteligente (reintentos, canales múltiples, prioridad).
                await _emailService.SendTemplatedEmailAsync(
                    invitation.Email,
                    "InclusiON - Invitacion para registro familiar",
                    "invitation",
                    replacements,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo enviar email de invitacion a {Email}", invitation.Email);
            }
        }
    }
}
