using Microsoft.AspNetCore.Http;
using ShiftOne.Core.Common.Constants;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Shared.Constants;
using System.Security.Claims;

namespace ShiftOne.Infrastructure.Providers.Security
{
    public sealed class TenantContext : ITenantContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TenantContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

        public Guid? UserId => TryReadGuid(AppConstants.Claims.UserIdentifier);

        public Guid? CompanyId
        {
            get
            {
                if (IsPlatformAdmin && _httpContextAccessor.HttpContext?.Request.Headers.TryGetValue("X-Company-Id", out var selectedCompanyId) == true)
                {
                    if (Guid.TryParse(selectedCompanyId.ToString(), out var headerCompanyId))
                    {
                        return headerCompanyId;
                    }
                }

                return TryReadGuid(AppConstants.Claims.CompanyId);
            }
        }

        public Guid? BranchId => TryReadGuid(AppConstants.Claims.BranchId);

        public IReadOnlyCollection<string> Roles
        {
            get
            {
                if (User == null) return [];
                return User.Claims
                    .Where(c => c.Type == ClaimTypes.Role || 
                                c.Type == "role" || 
                                c.Type == "roles" || 
                                c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")
                    .Select(c => c.Value)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
        }

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
        public bool IsSuperAdmin => HasRole(ShiftOne.Shared.Constants.Roles.SuperAdmin.ToString());
        public bool IsPlatformAdmin => IsSuperAdmin || HasRole(ShiftOne.Shared.Constants.Roles.Admin.ToString());
        public bool IsCompanyAdmin => HasRole(ShiftOne.Shared.Constants.Roles.CompanyAdmin.ToString());
        public bool IsHr => HasRole(ShiftOne.Shared.Constants.Roles.HR.ToString());
        public bool IsEmployee => HasRole(ShiftOne.Shared.Constants.Roles.Employee.ToString());
        public bool IsTenantScoped => !IsPlatformAdmin;

        public Guid RequireUserId() => UserId ?? throw new UnauthorizedAccessException("Authenticated user context is required.");

        public Guid RequireCompanyId() => CompanyId ?? throw new UnauthorizedAccessException("Company tenant context is required.");

        public bool CanAccessCompany(Guid? companyId)
        {
            if (IsPlatformAdmin)
            {
                return true;
            }

            return companyId.HasValue && CompanyId.HasValue && companyId.Value == CompanyId.Value;
        }

        public Guid? ResolveCompanyId(Guid? requestedCompanyId)
        {
            if (IsPlatformAdmin)
            {
                return requestedCompanyId ?? CompanyId;
            }

            if (!CompanyId.HasValue)
            {
                throw new UnauthorizedAccessException("Company tenant context is required.");
            }

            if (requestedCompanyId.HasValue && requestedCompanyId.Value != CompanyId.Value)
            {
                throw new UnauthorizedAccessException("Requested company is outside the authenticated tenant.");
            }

            return CompanyId.Value;
        }

        private bool HasRole(string role)
        {
            if (User == null) return false;
            if (User.IsInRole(role)) return true;
            return User.Claims.Any(c => 
                (c.Type == ClaimTypes.Role || 
                 c.Type == "role" || 
                 c.Type == "roles" || 
                 c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role") && 
                string.Equals(c.Value, role, StringComparison.OrdinalIgnoreCase));
        }

        private Guid? TryReadGuid(string claimType)
        {
            var value = User?.FindFirst(claimType)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}