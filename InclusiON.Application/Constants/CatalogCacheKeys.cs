namespace InclusiON.Application.Constants;

/// <summary>
/// Cache keys para catálogos — compartidos entre handlers (escritura en IMemoryCache)
/// y CatalogAdminController (invalidación). Mantenerlos sincronizados aquí evita stale data.
/// </summary>
public static class CatalogCacheKeys
{
    public const string SkillAreas           = "Catalog_SkillAreas";
    public const string DisabilityTypes      = "Catalog_DisabilityTypes";
    public const string ActivityCategories   = "Catalog_ActivityCategories";
    public const string AutonomyLevels       = "Catalog_AutonomyLevels";
    public const string ReportTypes          = "Catalog_ReportTypes";
    public const string ActivityTemplateTypes = "Catalog_ActivityTemplateTypes";
    public const string LoginMethods         = "LoginMethods_Active";

    /// <summary>Todas las keys — usadas para invalidación en bloque.</summary>
    public static readonly IReadOnlyList<string> All =
    [
        SkillAreas, DisabilityTypes, ActivityCategories,
        AutonomyLevels, ReportTypes, ActivityTemplateTypes, LoginMethods
    ];
}
