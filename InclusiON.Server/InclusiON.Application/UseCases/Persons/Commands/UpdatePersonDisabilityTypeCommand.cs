namespace InclusiON.Application.UseCases.Persons.Commands
{
    public record UpdatePersonDisabilityTypeCommand(Guid PersonId, int DisabilityTypeId);
}
