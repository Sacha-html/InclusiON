using InclusiON.DTOs.Common;

namespace InclusiON.DTOs.Requests.Activities
{
    public class GetActivitiesRequest : PagedRequest
    {
        public string? Search { get; set; }
        public int? CategoryId { get; set; }
        public int? SkillAreaId { get; set; }
        public int? TemplateTypeId { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsStandard { get; set; }
        public bool? IsTemplate { get; set; }

        // Las columnas de esta lista (título, tipo, categoría, etc.) no forman parte del
        // enum SortField compartido: bindear SortBy contra ese enum hacía fallar el
        // model binding (400) apenas se ordenaba por cualquier columna que no fuera "Title".
        // Se ocultan las propiedades de la base y se usan como texto libre.
        public new string? SortBy { get; set; }
        public new string SortDirection { get; set; } = "ASC";
    }
}
