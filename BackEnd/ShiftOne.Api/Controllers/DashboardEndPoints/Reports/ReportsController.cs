using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests.Reports;

namespace ShiftOne.Api.Controllers.DashboardEndPoints.Reports
{
    [Route("api/reports")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private static readonly HashSet<string> PlatformOnlyReports = new(StringComparer.OrdinalIgnoreCase)
        {
            "companies",
            "subscriptions",
            "plan-usage"
        };

        private readonly IReportService _reportService;
        private readonly ITenantContext _tenantContext;

        public ReportsController(IReportService reportService, ITenantContext tenantContext)
        {
            _reportService = reportService;
            _tenantContext = tenantContext;
        }

        /// <summary>Get attendance report rows with tenant, date, employee, branch, status, search, sorting, and pagination filters.</summary>
        [HasPermission(Permissions.Reports.View)]
        [HttpGet("attendance")]
        public async Task<IActionResult> Attendance([FromQuery] ReportRequest request) => (await _reportService.GetReportAsync("attendance", request)).ToActionResult();

        /// <summary>Get employee report rows with tenant, branch, status, role/search, date joined, sorting, and pagination filters.</summary>
        [HasPermission(Permissions.Reports.View)]
        [HttpGet("employees")]
        public async Task<IActionResult> Employees([FromQuery] ReportRequest request) => (await _reportService.GetReportAsync("employees", request)).ToActionResult();

        /// <summary>Get company report rows including current plan, branch count, employee count, subscription status, and expiration date.</summary>
        [HasPermission(Permissions.Reports.View)]
        [HttpGet("companies")]
        public async Task<IActionResult> Companies([FromQuery] ReportRequest request)
        {
            var forbidden = ForbidPlatformReportForTenant("companies");
            if (forbidden != null) return forbidden;

            return (await _reportService.GetReportAsync("companies", request)).ToActionResult();
        }

        /// <summary>Get branch report rows including company, employee count, today's attendance count, geofence status, and active status.</summary>
        [HasPermission(Permissions.Reports.View)]
        [HttpGet("branches")]
        public async Task<IActionResult> Branches([FromQuery] ReportRequest request) => (await _reportService.GetReportAsync("branches", request)).ToActionResult();

        /// <summary>Get subscription report rows including company, plan, price, status, dates, and remaining days.</summary>
        [HasPermission(Permissions.Reports.View)]
        [HttpGet("subscriptions")]
        public async Task<IActionResult> Subscriptions([FromQuery] ReportRequest request)
        {
            var forbidden = ForbidPlatformReportForTenant("subscriptions");
            if (forbidden != null) return forbidden;

            return (await _reportService.GetReportAsync("subscriptions", request)).ToActionResult();
        }

        /// <summary>Get plan usage report rows including companies, employees, branches, and average usage.</summary>
        [HasPermission(Permissions.Reports.View)]
        [HttpGet("plan-usage")]
        public async Task<IActionResult> PlanUsage([FromQuery] ReportRequest request)
        {
            var forbidden = ForbidPlatformReportForTenant("plan-usage");
            if (forbidden != null) return forbidden;

            return (await _reportService.GetReportAsync("plan-usage", request)).ToActionResult();
        }

        /// <summary>Export a report using the reporting engine. XLSX is implemented; PDF is future-ready.</summary>
        [HasPermission(Permissions.Reports.Export)]
        [HttpGet("{reportKey}/export")]
        public async Task<IActionResult> Export([FromRoute] string reportKey, [FromQuery] string format, [FromQuery] ReportRequest request)
        {
            var forbidden = ForbidPlatformReportForTenant(reportKey);
            if (forbidden != null) return forbidden;

            var file = await _reportService.ExportReportAsync(reportKey, request, format);
            return File(file.Content, file.ContentType, file.FileName);
        }

        private IActionResult? ForbidPlatformReportForTenant(string reportKey)
        {
            return PlatformOnlyReports.Contains(reportKey) && !_tenantContext.IsPlatformAdmin ? Forbid() : null;
        }
    }
}
