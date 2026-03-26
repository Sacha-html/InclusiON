namespace InclusiON.Application.UseCases.Institutions.Commands
{
    public record UpdateInstitutionCommand(
        int Id,
        string Name,
        string? Address,
        string? Phone,
        string? Email
    );
}
