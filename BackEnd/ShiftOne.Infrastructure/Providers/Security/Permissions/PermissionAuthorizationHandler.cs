using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using ShiftOne.Infrastructure.Persistence;
using ShiftOne.Core.Common.Constants;
using ShiftOne.Shared.Constants;
using System.Security.Claims;

namespace ShiftOne.Infrastructure.Providers.Security.Permissions
{
    public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _memoryCache;

        public PermissionAuthorizationHandler(ApplicationDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            if (context.User?.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var userIdClaim = context.User.FindFirst(AppConstants.Claims.UserIdentifier)?.Value;
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return;
            }

            // Cache user active status and lockout check
            var userStatusCacheKey = $"user-status-{userId}";
            var isUserActive = await _memoryCache.GetOrCreateAsync(userStatusCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                var user = await _context.ApplicationUsers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(applicationUser => applicationUser.Id == userId);

                return user != null && user.IsActive && (user.LockoutEnd == null || user.LockoutEnd <= DateTimeOffset.UtcNow);
            });

            if (!isUserActive)
            {
                return;
            }

            var roleNames = context.User.FindAll(ClaimTypes.Role)
                .Select(claim => claim.Value)
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct()
                .ToList();

            if (roleNames.Count == 0)
            {
                return;
            }

            if (roleNames.Any(roleName => string.Equals(roleName, Roles.SuperAdmin.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                context.Succeed(requirement);
                return;
            }

            // Cache permission checks by role set
            var sortedRoles = string.Join(",", roleNames.OrderBy(r => r));
            var permissionCacheKey = $"user-permission-{sortedRoles}-{requirement.Permission}";

            var hasPermission = await _memoryCache.GetOrCreateAsync(permissionCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return await _context.RolePermissions
                    .AsNoTracking()
                    .AnyAsync(rolePermission =>
                        rolePermission.Permission.Name == requirement.Permission &&
                        rolePermission.Permission.Name != string.Empty &&
                        rolePermission.Role.IsActive &&
                        rolePermission.Role.Name != null &&
                        roleNames.Contains(rolePermission.Role.Name));
            });

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
