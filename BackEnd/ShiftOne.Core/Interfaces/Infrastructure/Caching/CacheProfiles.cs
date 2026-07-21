using Microsoft.Extensions.Caching.Memory;

namespace ShiftOne.Core.Interfaces.Infrastructure.Caching
{
    public static class CacheProfiles
    {
        public static readonly CacheProfile Short = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2),
            SlidingExpiration = TimeSpan.FromSeconds(45),
            Priority = CacheItemPriority.Normal
        };

        public static readonly CacheProfile Standard = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
            SlidingExpiration = TimeSpan.FromMinutes(2),
            Priority = CacheItemPriority.Normal
        };

        public static readonly CacheProfile Reference = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20),
            SlidingExpiration = TimeSpan.FromMinutes(5),
            Priority = CacheItemPriority.High
        };

        public static readonly CacheProfile Dashboard = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1),
            SlidingExpiration = TimeSpan.FromSeconds(30),
            Priority = CacheItemPriority.High
        };

        public static readonly CacheProfile Reports = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3),
            SlidingExpiration = TimeSpan.FromMinutes(1),
            Priority = CacheItemPriority.Normal
        };
    }
}
