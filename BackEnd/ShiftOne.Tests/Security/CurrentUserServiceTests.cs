using Microsoft.AspNetCore.Http;
using ShiftOne.Core.Common.Constants;
using ShiftOne.Infrastructure.Providers.Security;
using ShiftOne.Shared.Constants;
using System.Security.Claims;

namespace ShiftOne.Tests.Security
{
    public class CurrentUserServiceTests
    {
        [Fact]
        public void CurrentUserId_ReturnsNull_WhenClaimIsMissingOrInvalid()
        {
            var accessor = AccessorWithClaims(new Claim(AppConstants.Claims.UserIdentifier, "not-a-guid"));
            var service = CreateService(accessor);

            Assert.Null(service.CurrentUserId);
        }

        [Fact]
        public void CurrentCompanyId_ReturnsCompanyId_WhenClaimIsValid()
        {
            var companyId = Guid.NewGuid();
            var accessor = AccessorWithClaims(new Claim(AppConstants.Claims.CompanyId, companyId.ToString()));
            var service = CreateService(accessor);

            Assert.Equal(companyId, service.CurrentCompanyId);
        }

        [Fact]
        public void CurrentCompanyId_ReturnsNull_WhenClaimIsMissingOrInvalid()
        {
            var accessor = AccessorWithClaims(new Claim(AppConstants.Claims.CompanyId, "not-a-guid"));
            var service = CreateService(accessor);

            Assert.Null(service.CurrentCompanyId);
        }

        [Theory]
        [InlineData(nameof(Roles.CompanyAdmin))]
        [InlineData(nameof(Roles.HR))]
        public void CurrentCompanyId_IgnoresCompanyHeader_ForTenantRoles(string role)
        {
            var companyId = Guid.NewGuid();
            var forgedCompanyId = Guid.NewGuid();
            var context = ContextWithClaims(
                new Claim(AppConstants.Claims.CompanyId, companyId.ToString()),
                new Claim(ClaimTypes.Role, role));
            context.Request.Headers["X-Company-Id"] = forgedCompanyId.ToString();
            var service = CreateService(new HttpContextAccessor { HttpContext = context });

            Assert.Equal(companyId, service.CurrentCompanyId);
        }

        [Fact]
        public void CurrentCompanyId_UsesCompanyHeader_ForPlatformAdmin()
        {
            var companyId = Guid.NewGuid();
            var selectedCompanyId = Guid.NewGuid();
            var context = ContextWithClaims(
                new Claim(AppConstants.Claims.CompanyId, companyId.ToString()),
                new Claim(ClaimTypes.Role, nameof(Roles.SuperAdmin)));
            context.Request.Headers["X-Company-Id"] = selectedCompanyId.ToString();
            var service = CreateService(new HttpContextAccessor { HttpContext = context });

            Assert.Equal(selectedCompanyId, service.CurrentCompanyId);
        }

        [Fact]
        public void IsActived_ReturnsNull_WhenClaimIsMissingOrInvalid()
        {
            var accessor = AccessorWithClaims(new Claim(AppConstants.Claims.IsActive, "not-a-bool"));
            var service = CreateService(accessor);

            Assert.Null(service.IsActived);
        }

        private static CurrentUserService CreateService(IHttpContextAccessor accessor) => new(accessor, new TenantContext(accessor));

        private static HttpContextAccessor AccessorWithClaims(params Claim[] claims) => new() { HttpContext = ContextWithClaims(claims) };

        private static DefaultHttpContext ContextWithClaims(params Claim[] claims) => new()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
        };
    }
}