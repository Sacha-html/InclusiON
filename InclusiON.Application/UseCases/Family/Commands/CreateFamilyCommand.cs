namespace InclusiON.Application.UseCases.Family.Commands
{
    public record CreateFamilyCommand(
        string FirstName,
        string LastName,
        string Email,
        string? DocumentNumber,
        string? Phone,
        string? Relationship
    );
}
