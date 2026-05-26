namespace InclusiON.Domain.Enums
{
    /// <summary>
    /// Identificadores de los estados de asignación de actividad (coinciden con ActivityAssignmentStatuses.Id).
    /// </summary>
    public static class AssignmentStatuses
    {
        public const int Pendiente  = 1;
        public const int EnProgreso = 2;
        public const int Completada = 3;
        public const int Cancelada  = 4;

        /// <summary>
        /// Nombres de los estados tal como están en la base de datos (ActivityAssignmentStatuses.Name).
        /// Usar en lugar de strings literales en handlers y tests.
        /// </summary>
        public static class Names
        {
            public const string Pendiente  = "Pendiente";
            public const string EnProgreso = "En Progreso";
            public const string Completada = "Completada";
            public const string Cancelada  = "Cancelada";
        }
    }
}
