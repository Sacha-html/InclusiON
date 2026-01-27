namespace InclusiON.ApplicationBusiness.UseCases.Persons.Commands
{
    /// <summary>
    /// Comando para actualizar el metodo de login de una persona con discapacidad.
    /// </summary>
    public record UpdateLoginMethodCommand(
        Guid UserId,
        int LoginMethodId,
        string? Pin = null,
        Guid? SupervisorUserId = null
    );
}
