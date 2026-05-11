namespace InclusiON.Application.UseCases.Invitations.Queries
{
    public record GetInvitationsQuery(
        Guid? ProfessionalId = null,
        List<int>? InstitutionIds = null,
        int Page = 1,
        int PageSize = 10
    );
}
