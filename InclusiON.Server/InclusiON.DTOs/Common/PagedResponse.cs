namespace InclusiON.DTOs.Common
{
    /// <summary>Permite que filtros/middleware lean el total sin conocer el tipo genérico.</summary>
    public interface IHasTotalCount
    {
        int TotalRecords { get; }
        int TotalPages   { get; }
        int CurrentPage  { get; }
    }

    public class PagedResponse<T> : IHasTotalCount
    {
        public List<T> Data { get; set; } = new();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
    }
}
