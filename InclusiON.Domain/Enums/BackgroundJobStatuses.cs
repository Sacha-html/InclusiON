namespace InclusiON.Domain.Enums;

public static class BackgroundJobStatuses
{
    public const int Pending   = 1;
    public const int Running   = 2;
    public const int Completed = 3;
    public const int Failed    = 4;
    public const int Cancelled = 5;

    public static class Names
    {
        public const string Pending   = "Pendiente";
        public const string Running   = "En Proceso";
        public const string Completed = "Completado";
        public const string Failed    = "Fallido";
        public const string Cancelled = "Cancelado";
    }
}
