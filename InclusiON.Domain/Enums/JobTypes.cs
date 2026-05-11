namespace InclusiON.Domain.Enums;

public static class JobTypes
{
    public const int Embedding          = 1;
    public const int Email              = 2;
    public const int Push               = 3;
    public const int AdaptiveAdjustment = 4;
    public const int TemplateGeneration = 5;

    public static class Names
    {
        public const string Embedding          = "Embedding";
        public const string Email              = "Email";
        public const string Push               = "Notificacion Push";
        public const string AdaptiveAdjustment = "Ajuste Adaptativo";
        public const string TemplateGeneration = "Generacion de Templates";
    }
}
