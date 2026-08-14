namespace InclusiON.Application.UseCases.Activities.Commands
{
    public record AutoAssignActivityCommand(
        int ActivityId,
        Guid PersonId
    );
}
