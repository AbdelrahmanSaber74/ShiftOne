using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using ShiftOne.Core.Interfaces.Infrastructure.Caching;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Api.Filters
{
    public sealed class EnterpriseCacheFilter : IAsyncActionFilter
    {
        private static readonly IReadOnlyDictionary<string, string[]> InvalidationMap = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            [CacheResources.Companies] = [CacheResources.Companies, CacheResources.Branches, CacheResources.Employees, CacheResources.Subscriptions, CacheResources.Dashboard, CacheResources.Reports],
            [CacheResources.Branches] = [CacheResources.Branches, CacheResources.Employees, CacheResources.Attendance, CacheResources.Dashboard, CacheResources.Reports],
            [CacheResources.Employees] = [CacheResources.Employees, CacheResources.Attendance, CacheResources.Users, CacheResources.Dashboard, CacheResources.Reports],
            [CacheResources.Attendance] = [CacheResources.Attendance, CacheResources.Employees, CacheResources.Dashboard, CacheResources.Reports],
            [CacheResources.Plans] = [CacheResources.Plans, CacheResources.Companies, CacheResources.Subscriptions, CacheResources.Dashboard, CacheResources.Reports],
            [CacheResources.Subscriptions] = [CacheResources.Subscriptions, CacheResources.Companies, CacheResources.Dashboard, CacheResources.Reports],
            [CacheResources.Users] = [CacheResources.Users, CacheResources.Employees, CacheResources.Dashboard, CacheResources.Reports],
            [CacheResources.Roles] = [CacheResources.Roles, CacheResources.Permissions, CacheResources.Users, CacheResources.Dashboard],
            [CacheResources.Permissions] = [CacheResources.Permissions, CacheResources.Roles, CacheResources.Users, CacheResources.Dashboard],
            [CacheResources.Reports] = [CacheResources.Reports],
        };

        private readonly ICacheService _cacheService;
        private readonly ICacheKeyBuilder _cacheKeyBuilder;
        private readonly ICacheInvalidationService _cacheInvalidationService;
        private readonly ITenantContext _tenantContext;
        private readonly ILogger<EnterpriseCacheFilter> _logger;

        public EnterpriseCacheFilter(
            ICacheService cacheService,
            ICacheKeyBuilder cacheKeyBuilder,
            ICacheInvalidationService cacheInvalidationService,
            ITenantContext tenantContext,
            ILogger<EnterpriseCacheFilter> logger)
        {
            _cacheService = cacheService;
            _cacheKeyBuilder = cacheKeyBuilder;
            _cacheInvalidationService = cacheInvalidationService;
            _tenantContext = tenantContext;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var request = context.HttpContext.Request;
            var resource = ResolveResource(request.Path.Value ?? string.Empty);

            if (resource is null)
            {
                await next();
                return;
            }

            if (HttpMethods.IsGet(request.Method) && ShouldCacheRead(request.Path.Value ?? string.Empty))
            {
                var key = await BuildKeyAsync(request, resource, context.HttpContext.RequestAborted);
                if (request.Headers.TryGetValue("Cache-Control", out var cacheControl) && 
                    cacheControl.ToString().Contains("no-cache", StringComparison.OrdinalIgnoreCase))
                {
                    _cacheService.Remove(key);
                }
                var cached = await _cacheService.GetOrCreateAsync(key, async () =>
                {
                    var executed = await next();
                    return ToCacheResult(executed.Result);
                }, ResolveProfile(resource), context.HttpContext.RequestAborted);

                if (cached is not null)
                {
                    context.Result = new ObjectResult(cached.Value)
                    {
                        StatusCode = cached.StatusCode
                    };
                }

                return;
            }

            var resultContext = await next();
            if (IsSuccessfulWrite(request.Method, resultContext.Result))
            {
                await InvalidateAsync(resource);
            }
        }

        private async Task<string> BuildKeyAsync(HttpRequest request, string resource, CancellationToken cancellationToken)
        {
            var filters = request.Query
                .Where(item => !IsPromotedFilter(item.Key))
                .ToDictionary(item => item.Key, item => (object?)item.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            var query = request.Query;
            return await _cacheKeyBuilder.BuildAsync(new CacheKeyRequest
            {
                Resource = resource,
                Operation = ResolveOperation(request.Path.Value ?? string.Empty),
                TenantId = ResolveTenantId(),
                CompanyId = TryGetGuid(query, "companyId"),
                BranchId = TryGetGuid(query, "branchId"),
                EmployeeId = TryGetGuid(query, "employeeId"),
                UserId = _tenantContext.UserId,
                Language = request.Headers.AcceptLanguage.FirstOrDefault(),
                Page = TryGetInt(query, "page"),
                PageSize = TryGetInt(query, "pageSize"),
                SortBy = query.TryGetValue("sortBy", out var sortBy) ? sortBy.ToString() : null,
                SortDirection = query.TryGetValue("sortDirection", out var sortDirection) ? sortDirection.ToString() : null,
                Search = query.TryGetValue("keyword", out var keyword) ? keyword.ToString() : null,
                Status = query.TryGetValue("status", out var status) ? status.ToString() : query.TryGetValue("isActive", out var isActive) ? isActive.ToString() : null,
                Filters = filters
            }, cancellationToken);
        }

        private async Task InvalidateAsync(string resource)
        {
            if (!InvalidationMap.TryGetValue(resource, out var resources))
            {
                resources = [resource, CacheResources.Dashboard, CacheResources.Reports];
            }

            await _cacheInvalidationService.InvalidateAsync(ResolveTenantId(), resources);
        }

        private Guid? ResolveTenantId()
        {
            return _tenantContext.CompanyId;
        }

        private static ShiftOne.Core.Interfaces.Infrastructure.Caching.CacheProfile ResolveProfile(string resource)
        {
            return resource switch
            {
                CacheResources.Reports => CacheProfiles.Reports,
                CacheResources.Dashboard => CacheProfiles.Dashboard,
                CacheResources.Plans or CacheResources.Roles or CacheResources.Permissions => CacheProfiles.Reference,
                _ => CacheProfiles.Standard
            };
        }

        private static bool ShouldCacheRead(string path)
        {
            return !path.Contains("/export", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsSuccessfulWrite(string method, IActionResult? result)
        {
            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsOptions(method))
            {
                return false;
            }

            return result switch
            {
                ObjectResult { Value: GeneralResponse response } => response.Success,
                ObjectResult objectResult => objectResult.StatusCode is null or >= 200 and < 300,
                StatusCodeResult statusCodeResult => statusCodeResult.StatusCode is >= 200 and < 300,
                EmptyResult => true,
                _ => false
            };
        }

        private static CachedActionResult? ToCacheResult(IActionResult? result)
        {
            return result switch
            {
                ObjectResult { Value: GeneralResponse response } objectResult when response.Success => new CachedActionResult(response, objectResult.StatusCode ?? response.StatusCode),
                ObjectResult objectResult when objectResult.StatusCode is null or >= 200 and < 300 => new CachedActionResult(objectResult.Value, objectResult.StatusCode ?? 200),
                _ => null
            };
        }

        private static string? ResolveResource(string path)
        {
            var normalized = path.ToLowerInvariant();
            if (normalized.Contains("/reports")) return CacheResources.Reports;
            if (normalized.Contains("/companies")) return CacheResources.Companies;
            if (normalized.Contains("/branches")) return CacheResources.Branches;
            if (normalized.Contains("/employees/attendance") || normalized.Contains("/attendance")) return CacheResources.Attendance;
            if (normalized.Contains("/employees")) return CacheResources.Employees;
            if (normalized.Contains("/plans")) return CacheResources.Plans;
            if (normalized.Contains("/subscriptions")) return CacheResources.Subscriptions;
            if (normalized.Contains("/roles")) return CacheResources.Roles;
            if (normalized.Contains("/permissions")) return CacheResources.Permissions;
            if (normalized.Contains("/admin/getallusers") || normalized.Contains("/admin/getuserbyid") || normalized.Contains("/admin/approve") || normalized.Contains("/admin/unapprove")) return CacheResources.Users;
            return null;
        }

        private static string ResolveOperation(string path)
        {
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Any(segment => Guid.TryParse(segment, out _)) || path.Contains("getuserbyid", StringComparison.OrdinalIgnoreCase))
            {
                return "details";
            }

            return "list";
        }

        private static bool IsPromotedFilter(string key)
        {
            return key.Equals("page", StringComparison.OrdinalIgnoreCase)
                || key.Equals("pageSize", StringComparison.OrdinalIgnoreCase)
                || key.Equals("keyword", StringComparison.OrdinalIgnoreCase)
                || key.Equals("sortBy", StringComparison.OrdinalIgnoreCase)
                || key.Equals("sortDirection", StringComparison.OrdinalIgnoreCase)
                || key.Equals("status", StringComparison.OrdinalIgnoreCase)
                || key.Equals("isActive", StringComparison.OrdinalIgnoreCase)
                || key.Equals("companyId", StringComparison.OrdinalIgnoreCase)
                || key.Equals("branchId", StringComparison.OrdinalIgnoreCase)
                || key.Equals("employeeId", StringComparison.OrdinalIgnoreCase);
        }

        private static Guid? TryGetGuid(IQueryCollection query, string key)
        {
            return query.TryGetValue(key, out var value) && Guid.TryParse(value.ToString(), out var id) ? id : null;
        }

        private static int? TryGetInt(IQueryCollection query, string key)
        {
            return query.TryGetValue(key, out var value) && int.TryParse(value.ToString(), out var number) ? number : null;
        }

        private sealed record CachedActionResult(object? Value, int StatusCode);
    }
}

