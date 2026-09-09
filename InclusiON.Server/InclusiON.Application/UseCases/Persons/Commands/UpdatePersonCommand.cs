namespace InclusiON.Application.UseCases.Persons.Commands
{
    public record UpdatePersonCommand(
        Guid PersonId,
        string? FirstName,
        string? LastName,
        string? DocumentNumber,
        DateTime? BirthDate,
        string? PhotoUrl
    );
}
