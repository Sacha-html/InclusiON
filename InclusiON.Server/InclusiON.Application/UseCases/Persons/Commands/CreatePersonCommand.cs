namespace InclusiON.Application.UseCases.Persons.Commands
{
    public record CreatePersonCommand(
        string FirstName,
        string LastName,
        string? DocumentNumber,
        DateTime BirthDate,
        string? PhotoUrl
    );
}
