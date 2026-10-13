namespace InclusiON.Application.UseCases.Invitations.Commands
{
    public record CancelInvitationCommand(
        string Code,
        Guid UserId,
        string UserRole,
        Guid? ProfessionalId
    );
}
