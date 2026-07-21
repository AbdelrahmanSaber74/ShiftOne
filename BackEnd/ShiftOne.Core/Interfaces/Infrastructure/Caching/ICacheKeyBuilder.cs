namespace ShiftOne.Core.Interfaces.Infrastructure.Caching
{
    public interface ICacheKeyBuilder
    {
        Task<string> BuildAsync(CacheKeyRequest request, CancellationToken cancellationToken = default);
    }
}
