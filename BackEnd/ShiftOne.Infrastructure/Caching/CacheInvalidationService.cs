using Microsoft.Extensions.Logging;
using ShiftOne.Core.Interfaces.Infrastructure.Caching;

namespace ShiftOne.Infrastructure.Caching
{
    public sealed class CacheInvalidationService : ICacheInvalidationService
    {
        private readonly ICacheVersionManager _cacheVersionManager;
        private readonly ILogger<CacheInvalidationService> _logger;

        public CacheInvalidationService(ICacheVersionManager cacheVersionManager, ILogger<CacheInvalidationService> logger)
        {
            _cacheVersionManager = cacheVersionManager;
            _logger = logger;
        }

        public async Task InvalidateAsync(Guid? tenantId, params string[] resources)
        {
            var uniqueResources = resources
                .Where(resource => !string.IsNullOrWhiteSpace(resource))
                .Select(resource => resource.Trim().ToLowerInvariant())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var resource in uniqueResources)
            {
                await _cacheVersionManager.IncrementAsync(resource, tenantId);
                await _cacheVersionManager.IncrementAsync(resource, null);
            }

            _logger.LogInformation("Cache Invalidation: tenant {TenantId}, resources {Resources}", tenantId?.ToString() ?? "global", string.Join(",", uniqueResources));
        }
    }
}
