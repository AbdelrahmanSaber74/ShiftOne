namespace ShiftOne.Core.Interfaces.Infrastructure.Caching
{
    public interface ICacheInvalidationService
    {
        Task InvalidateAsync(Guid? tenantId, params string[] resources);
    }
}
