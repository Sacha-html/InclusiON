namespace InclusiON.Application.UseCases.Professionals.Commands
{
    public record SuspendInactiveProfessionalsCommand(int InactiveDays = 90);
}
