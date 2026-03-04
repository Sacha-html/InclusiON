namespace InclusiON.DTOs.Responses.Persons
{
    /// <summary>
    /// Respuesta al actualizar el metodo de login de una persona.
    /// </summary>
    public class UpdateLoginMethodResponse
    {
        /// <summary>
        /// Indica si la operacion fue exitosa.
        /// </summary>
        public bool Updated { get; set; }

        /// <summary>
        /// ID del nuevo metodo de login asignado.
        /// </summary>
        public int LoginMethodId { get; set; }

        /// <summary>
        /// Nombre del metodo de login asignado.
        /// </summary>
        public string LoginMethodName { get; set; } = string.Empty;
    }
}
