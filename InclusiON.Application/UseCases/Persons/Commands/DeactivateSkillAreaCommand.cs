namespace InclusiON.Application.UseCases.Persons.Commands
{
    public record DeactivateSkillAreaCommand(Guid PersonId, int SkillAreaId);
}
