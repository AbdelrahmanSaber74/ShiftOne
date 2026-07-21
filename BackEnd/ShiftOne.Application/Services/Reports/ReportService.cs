using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Core.Interfaces.Infrastructure.Reports;
using ShiftOne.Shared.Requests.Reports;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.Reports;

namespace ShiftOne.Application.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly IReportQueryProvider _queryProvider;
        private readonly IReportExportService _exportService;
        private readonly ITenantContext _tenantContext;

        public ReportService(IReportQueryProvider queryProvider, IReportExportService exportService, ITenantContext tenantContext)
        {
            _queryProvider = queryProvider;
            _exportService = exportService;
            _tenantContext = tenantContext;
        }

        public async Task<GeneralResponse> GetReportAsync(string reportKey, ReportRequest request)
        {
            var tenantValidation = ValidateTenant(request);
            if (tenantValidation != null)
            {
                return tenantValidation;
            }

            try
            {
                var result = await _queryProvider.QueryAsync(reportKey, NormalizeRequest(request));
                return GeneralResponse.Ok("Messages.Success", result.Rows, result.Page, result.PageSize, result.TotalCount);
            }
            catch (KeyNotFoundException)
            {
                return GeneralResponse.NotFound("Messages.ReportNotFound");
            }
        }

        public async Task<ReportExportFile> ExportReportAsync(string reportKey, ReportRequest request, string format)
        {
            var tenantValidation = ValidateTenant(request);
            if (tenantValidation != null)
            {
                return new ReportExportFile
                {
                    Content = System.Text.Encoding.UTF8.GetBytes(tenantValidation.Message),
                    ContentType = "text/plain",
                    FileName = "report-error.txt"
                };
            }

            var exportRequest = NormalizeRequest(request);
            exportRequest.Page = 1;
            exportRequest.PageSize = 5000;
            var result = await _queryProvider.QueryAsync(reportKey, exportRequest);
            return await _exportService.ExportAsync(result, format, exportRequest);
        }

        private GeneralResponse? ValidateTenant(ReportRequest request)
        {
            if (_tenantContext.IsPlatformAdmin)
            {
                return null;
            }

            if (!_tenantContext.CompanyId.HasValue)
            {
                return GeneralResponse.Unauthorized("Messages.CompanyContextRequired");
            }

            if (!_tenantContext.CanAccessCompany(request.CompanyId ?? _tenantContext.CompanyId))
            {
                return GeneralResponse.Unauthorized("Messages.CompanyContextRequired");
            }

            return null;
        }

        private static ReportRequest NormalizeRequest(ReportRequest request)
        {
            request.Page = request.SafePage;
            request.PageSize = request.SafePageSize;
            request.Keyword = string.IsNullOrWhiteSpace(request.Keyword) ? null : request.Keyword.Trim();
            request.Status = string.IsNullOrWhiteSpace(request.Status) ? null : request.Status.Trim();
            request.Role = string.IsNullOrWhiteSpace(request.Role) ? null : request.Role.Trim();
            request.SortBy = string.IsNullOrWhiteSpace(request.SortBy) ? null : request.SortBy.Trim();
            request.SortDirection = string.IsNullOrWhiteSpace(request.SortDirection) ? "asc" : request.SortDirection.Trim();
            return request;
        }
    }
}

