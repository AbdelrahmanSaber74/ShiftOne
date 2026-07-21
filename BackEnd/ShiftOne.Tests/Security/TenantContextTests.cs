using Microsoft.AspNetCore.Http;
using ShiftOne.Core.Common.Constants;
using ShiftOne.Infrastructure.Providers.Security;
using ShiftOne.Shared.Constants;
using System.Security.Claims;

namespace ShiftOne.Tests.Security
{
    public class TenantContextTests
    {
        [Theory]
        [InlineData(nameof(Roles.CompanyAdmin))]
        [InlineData(nameof(Roles.HR))]
        public void CompanyId_IgnoresForgedHeader_ForTenantScopedRoles(string role)
        {
            var companyId = Guid.NewGuid();
            var forgedCompanyId = Guid.NewGuid();
            var context = ContextWithClaims(
                new Claim(AppConstants.Claims.CompanyId, companyId.ToString()),
                new Claim(ClaimTypes.Role, role));
            context.Request.Headers["X-Company-Id"] = forgedCompanyId.ToString();

            var tenant = CreateTenant(context);

            Assert.Equal(companyId, tenant.CompanyId);
            Assert.True(tenant.IsTenantScoped);
            Assert.False(tenant.CanAccessCompany(forgedCompanyId));
            Assert.Equal(companyId, tenant.ResolveCompanyId(null));
        }

        [Theory]
        [InlineData(nameof(Roles.SuperAdmin))]
        [InlineData(nameof(Roles.Admin))]
        public void CompanyId_UsesSelectedHeader_ForPlatformRoles(string role)
        {
            var selectedCompanyId = Guid.NewGuid();
            var context = ContextWithClaims(new Claim(ClaimTypes.Role, role));
            context.Request.Headers["X-Company-Id"] = selectedCompanyId.ToString();

            var tenant = CreateTenant(context);

            Assert.True(tenant.IsPlatformAdmin);
            Assert.Equal(selectedCompanyId, tenant.CompanyId);
            Assert.True(tenant.CanAccessCompany(Guid.NewGuid()));
        }

        [Fact]
        public void RequireCompanyId_Throws_WhenTenantUserHasNoCompanyClaim()
        {
            var context = ContextWithClaims(new Claim(ClaimTypes.Role, nameof(Roles.HR)));
            var tenant = CreateTenant(context);

            Assert.Throws<UnauthorizedAccessException>(() => tenant.RequireCompanyId());
        }

        [Fact]
        public void ResolveCompanyId_Throws_WhenTenantUserRequestsAnotherCompany()
        {
            var companyId = Guid.NewGuid();
            var context = ContextWithClaims(
                new Claim(AppConstants.Claims.CompanyId, companyId.ToString()),
                new Claim(ClaimTypes.Role, nameof(Roles.CompanyAdmin)));
            var tenant = CreateTenant(context);

            Assert.Throws<UnauthorizedAccessException>(() => tenant.ResolveCompanyId(Guid.NewGuid()));
        }

        [Fact]
        public void BranchId_ReadsBranchClaim_WhenPresent()
        {
            var branchId = Guid.NewGuid();
            var context = ContextWithClaims(new Claim(AppConstants.Claims.BranchId, branchId.ToString()));
            var tenant = CreateTenant(context);

            Assert.Equal(branchId, tenant.BranchId);
        }

        private static TenantContext CreateTenant(DefaultHttpContext context) => new(new HttpContextAccessor { HttpContext = context });

        private static DefaultHttpContext ContextWithClaims(params Claim[] claims) => new()
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
        };
    }
}