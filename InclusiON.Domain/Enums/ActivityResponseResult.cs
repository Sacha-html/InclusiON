namespace InclusiON.Domain.Enums
{
    public enum ActivityResponseResult
    {
        /// <summary>Éxito: porcentaje de acierto >= 80%.</summary>
        Exito = 0,

        /// <summary>Parcial: porcentaje de acierto >= 50% y < 80%.</summary>
        Parcial = 1,

        /// <summary>Fallido: porcentaje de acierto < 50%.</summary>
        Fallido = 2
    }
}
