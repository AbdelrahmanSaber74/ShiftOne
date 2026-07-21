namespace ShiftOne.Shared.Requests.Reports
{
    public class ReportRequest : ReportFilter
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public Dictionary<string, string?> Filters { get; set; } = new();

        public int SafePage => Page < 1 ? 1 : Page;
        public int SafePageSize => PageSize < 1 ? 20 : Math.Min(PageSize, 500);
        public bool IsDescending => string.Equals(SortDirection, "desc", StringComparison.OrdinalIgnoreCase);
    }
}
