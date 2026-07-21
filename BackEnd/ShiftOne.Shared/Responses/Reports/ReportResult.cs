namespace ShiftOne.Shared.Responses.Reports
{
    public class ReportResult<T>
    {
        public string ReportKey { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime GeneratedOn { get; set; } = DateTime.UtcNow;
        public IReadOnlyList<T> Rows { get; set; } = Array.Empty<T>();
        public IReadOnlyList<ReportColumn> Columns { get; set; } = Array.Empty<ReportColumn>();
        public Dictionary<string, string> AppliedFilters { get; set; } = new();
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;
    }
}
