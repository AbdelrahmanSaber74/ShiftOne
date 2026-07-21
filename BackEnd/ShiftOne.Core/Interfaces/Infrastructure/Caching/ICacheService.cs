namespace ShiftOne.Core.Interfaces.Infrastructure.Caching
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, CacheProfile? profile = null, CancellationToken cancellationToken = default);
        void Remove(string key);
    }
}
