using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using ShiftOne.Core.Interfaces.Infrastructure.Caching;

namespace ShiftOne.Infrastructure.Caching
{
    public sealed class CacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CacheService> _logger;

        public CacheService(IMemoryCache memoryCache, ILogger<CacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, CacheProfile? profile = null, CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(key, out T? cachedValue) && cachedValue is not null)
            {
                _logger.LogInformation("Cache Hit: {CacheKey}", key);
                return cachedValue;
            }

            _logger.LogInformation("Cache Miss: {CacheKey}", key);
            var value = await factory();

            var cacheProfile = profile ?? CacheProfiles.Standard;
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = cacheProfile.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = cacheProfile.SlidingExpiration,
                Priority = cacheProfile.Priority
            };

            _memoryCache.Set(key, value, options);
            _logger.LogInformation("Cache Refresh: {CacheKey}", key);
            return value;
        }

        public void Remove(string key)
        {
            _memoryCache.Remove(key);
            _logger.LogInformation("Cache Invalidation: {CacheKey}", key);
        }
    }
}
