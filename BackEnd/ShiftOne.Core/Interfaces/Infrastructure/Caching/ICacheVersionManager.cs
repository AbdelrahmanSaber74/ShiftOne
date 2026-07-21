namespace ShiftOne.Core.Interfaces.Infrastructure.Caching
{
    public interface ICacheVersionManager
    {
        Task<long> GetVersionAsync(string resource, Guid? tenantId = null, CancellationToken cancellationToken = default);
        Task<long> IncrementAsync(string resource, Guid? tenantId = null, CancellationToken cancellationToken = default);
    }
}
