namespace InclusiON.Application.UseCases.Family.Commands
{
    public record UpdateFamilyCommand(
        Guid FamilyId,
        string FirstName,
        string LastName,
        string Email,
        string? DocumentNumber,
        string? Phone,
        string? Relationship
    );
}
