namespace ShiftOne.Core.Interfaces.Infrastructure.Caching
{
    public sealed class CacheKeyRequest
    {
        public required string Resource { get; init; }
        public required string Operation { get; init; }
        public Guid? TenantId { get; init; }
        public Guid? CompanyId { get; init; }
        public Guid? BranchId { get; init; }
        public Guid? EmployeeId { get; init; }
        public Guid? UserId { get; init; }
        public string? Language { get; init; }
        public int? Page { get; init; }
        public int? PageSize { get; init; }
        public string? SortBy { get; init; }
        public string? SortDirection { get; init; }
        public string? Search { get; init; }
        public string? Status { get; init; }
        public IReadOnlyDictionary<string, object?>? Filters { get; init; }
    }
}
