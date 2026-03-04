namespace InclusiON.DTOs.Common
{
    public class PagedRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public SortField? SortBy { get; set; }
        public string SortDirection { get; set; } = "DESC";

        public void Validate()
        {
            if (Page < 1) Page = 1;
            if (PageSize < 1) PageSize = 20;
            if (PageSize > 100) PageSize = 100;

            SortDirection = SortDirection?.ToUpperInvariant() switch
            {
                "DESC" => "DESC",
                _ => "ASC"
            };
        }

        public int Skip => (Page - 1) * PageSize;
    }
}
