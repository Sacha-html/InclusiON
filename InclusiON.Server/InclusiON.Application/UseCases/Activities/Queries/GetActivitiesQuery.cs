namespace InclusiON.Application.UseCases.Activities.Queries
{
    public record GetActivitiesQuery(
        Guid ProfessionalId,
        string? Search,
        int? CategoryId,
        int? SkillAreaId,
        int? TemplateTypeId,
        bool? IsActive,
        bool? IsStandard,
        bool? IsTemplate,
        int Page,
        int PageSize
    );
}
