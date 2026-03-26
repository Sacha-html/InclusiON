namespace InclusiON.Application.UseCases.Invitations.Commands
{
    public record CreateInvitationCommand(
        Guid ProfessionalId,
        Guid? PersonId,
        string Email,
        string? FirstName,
        string? LastName,
        string? Relationship,
        string? BaseUrl = null
    );
}
