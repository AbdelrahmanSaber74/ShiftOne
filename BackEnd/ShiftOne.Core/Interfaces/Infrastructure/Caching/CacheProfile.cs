using Microsoft.Extensions.Caching.Memory;

namespace ShiftOne.Core.Interfaces.Infrastructure.Caching
{
    public sealed class CacheProfile
    {
        public TimeSpan AbsoluteExpirationRelativeToNow { get; init; } = TimeSpan.FromMinutes(5);
        public TimeSpan? SlidingExpiration { get; init; }
        public CacheItemPriority Priority { get; init; } = CacheItemPriority.Normal;
    }
}
