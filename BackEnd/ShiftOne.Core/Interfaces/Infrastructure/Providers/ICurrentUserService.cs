namespace ShiftOne.Core.Interfaces.Infrastructure.Providers
{
    public interface ICurrentUserService
    {
        Guid? CurrentUserId { get; }
        Guid? CurrentCompanyId { get; }
        bool IsSuperAdmin { get; }
        bool IsPlatformAdmin { get; }
        bool IsCompanyAdmin { get; }
        bool IsHr { get; }
        string CurrentUserName { get; }
        string? CurrentIpAddress { get; }
        bool? IsActived { get; }
        string GetBaseUrl(string relativePath);
    }
}
