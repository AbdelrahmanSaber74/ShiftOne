using Microsoft.AspNetCore.Http;
using ShiftOne.Core.Common.Constants;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;

namespace ShiftOne.Infrastructure.Providers.Security
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantContext _tenantContext;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor, ITenantContext tenantContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenantContext = tenantContext;
        }

        public Guid? CurrentUserId => _tenantContext.UserId;

        public bool IsSuperAdmin => _tenantContext.IsSuperAdmin;

        public bool IsPlatformAdmin => _tenantContext.IsPlatformAdmin;

        public bool IsCompanyAdmin => _tenantContext.IsCompanyAdmin;

        public bool IsHr => _tenantContext.IsHr;

        public Guid? CurrentCompanyId => _tenantContext.CompanyId;

        public bool? IsActived
        {
            get
            {
                var userLangClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(AppConstants.Claims.IsActive)?.Value;
                return bool.TryParse(userLangClaim, out var state) ? state : null;
            }
        }

        public string CurrentUserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? string.Empty;

        public string? CurrentIpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        public string GetBaseUrl(string relativePath)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request == null) return relativePath;

            var baseUrl = $"{request.Scheme}://{request.Host}";
            return $"{baseUrl}{relativePath}";
        }
    }
}