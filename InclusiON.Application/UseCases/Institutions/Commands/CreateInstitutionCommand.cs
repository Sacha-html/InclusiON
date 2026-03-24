namespace InclusiON.Application.UseCases.Institutions.Commands
{
    public record CreateInstitutionCommand(
        string Name,
        string? Address,
        string? Phone,
        string? Email
    );
}
