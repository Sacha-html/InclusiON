namespace InclusiON.Application.UseCases.Persons.Commands
{
    public record UpdatePersonAccessConfigurationCommand(
        Guid PersonId, int AutonomyLevelId, int LoginMethodId, string? Pin,
        Guid? SupervisorUserId, string AvatarColor);
}
