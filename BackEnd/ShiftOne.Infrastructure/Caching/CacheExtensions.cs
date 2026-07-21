using ShiftOne.Core.Interfaces.Infrastructure.Caching;
using ShiftOne.Shared.Requests;

namespace ShiftOne.Infrastructure.Caching
{
    public static class CacheExtensions
    {
        public static CacheKeyRequest ListKey(string resource, Guid? tenantId, PaginationRequest request, string? keyword = null, string? status = null, IReadOnlyDictionary<string, object?>? filters = null)
        {
            return new CacheKeyRequest
            {
                Resource = resource,
                Operation = "list",
                TenantId = tenantId,
                Page = request.Page,
                PageSize = request.PageSize,
                Search = keyword,
                Status = status,
                Filters = filters
            };
        }

        public static CacheKeyRequest DetailsKey(string resource, Guid? tenantId, Guid id)
        {
            return new CacheKeyRequest
            {
                Resource = resource,
                Operation = "details",
                TenantId = tenantId,
                Filters = new Dictionary<string, object?> { ["id"] = id }
            };
        }
    }
}
