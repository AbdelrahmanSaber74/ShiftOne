namespace ShiftOne.Core.Interfaces.Infrastructure.Providers
{
    public interface ITenantContext
    {
        Guid? UserId { get; }
        Guid? CompanyId { get; }
        Guid? BranchId { get; }
        IReadOnlyCollection<string> Roles { get; }
        bool IsAuthenticated { get; }
        bool IsSuperAdmin { get; }
        bool IsPlatformAdmin { get; }
        bool IsCompanyAdmin { get; }
        bool IsHr { get; }
        bool IsEmployee { get; }
        bool IsTenantScoped { get; }

        Guid RequireUserId();
        Guid RequireCompanyId();
        bool CanAccessCompany(Guid? companyId);
        Guid? ResolveCompanyId(Guid? requestedCompanyId);
    }
}