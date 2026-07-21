using ShiftOne.Core.Interfaces.Infrastructure.Caching;

namespace ShiftOne.Infrastructure.Caching
{
    public sealed class CacheKeyBuilder : ICacheKeyBuilder
    {
        private readonly ICacheVersionManager _cacheVersionManager;

        public CacheKeyBuilder(ICacheVersionManager cacheVersionManager)
        {
            _cacheVersionManager = cacheVersionManager;
        }

        public async Task<string> BuildAsync(CacheKeyRequest request, CancellationToken cancellationToken = default)
        {
            var version = await _cacheVersionManager.GetVersionAsync(request.Resource, request.TenantId, cancellationToken);
            var parts = new List<string>
            {
                "tenant",
                Normalize(request.TenantId?.ToString("N") ?? "global"),
                Normalize(request.Resource),
                Normalize(request.Operation)
            };

            Add(parts, "company", request.CompanyId?.ToString("N"));
            Add(parts, "branch", request.BranchId?.ToString("N"));
            Add(parts, "employee", request.EmployeeId?.ToString("N"));
            Add(parts, "user", request.UserId?.ToString("N"));
            Add(parts, "lang", request.Language);
            Add(parts, "page", request.Page?.ToString());
            Add(parts, "size", request.PageSize?.ToString());
            Add(parts, "sort", request.SortBy);
            Add(parts, "dir", request.SortDirection);
            Add(parts, "search", request.Search);
            Add(parts, "status", request.Status);

            if (request.Filters is not null)
            {
                foreach (var filter in request.Filters.OrderBy(filter => filter.Key, StringComparer.OrdinalIgnoreCase))
                {
                    Add(parts, filter.Key, filter.Value?.ToString());
                }
            }

            parts.Add($"v{version}");
            return string.Join(":", parts.Where(part => !string.IsNullOrWhiteSpace(part)));
        }

        private static void Add(ICollection<string> parts, string name, string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            parts.Add(Normalize(name));
            parts.Add(Normalize(value));
        }

        private static string Normalize(string value)
        {
            return value.Trim().ToLowerInvariant().Replace(" ", "-");
        }
    }
}
