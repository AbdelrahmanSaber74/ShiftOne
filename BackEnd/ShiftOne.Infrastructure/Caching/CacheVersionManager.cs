using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ShiftOne.Core.Interfaces.Infrastructure.Caching;

namespace ShiftOne.Infrastructure.Caching
{
    public sealed class CacheVersionManager : ICacheVersionManager
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheVersionManager> _logger;

        public CacheVersionManager(IMemoryCache memoryCache, ILogger<CacheVersionManager> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public Task<long> GetVersionAsync(string resource, Guid? tenantId = null, CancellationToken cancellationToken = default)
        {
            var key = BuildVersionKey(resource, tenantId);
            var version = _memoryCache.GetOrCreate(key, entry =>
            {
                entry.Priority = CacheItemPriority.NeverRemove;
                return 1L;
            });

            return Task.FromResult(version);
        }

        public Task<long> IncrementAsync(string resource, Guid? tenantId = null, CancellationToken cancellationToken = default)
        {
            var key = BuildVersionKey(resource, tenantId);
            var current = _memoryCache.Get<long?>(key) ?? 1L;
            var next = current + 1;
            _memoryCache.Set(key, next, new MemoryCacheEntryOptions { Priority = CacheItemPriority.NeverRemove });
            _logger.LogInformation("Cache Version Increment: {Resource} tenant {TenantId} -> v{Version}", resource, tenantId?.ToString() ?? "global", next);
            return Task.FromResult(next);
        }

        private static string BuildVersionKey(string resource, Guid? tenantId)
        {
            var tenant = tenantId?.ToString("N") ?? "global";
            return $"cache-version:{tenant}:{resource}";
        }
    }
}
