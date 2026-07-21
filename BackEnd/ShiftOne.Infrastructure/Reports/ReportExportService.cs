using ShiftOne.Core.Interfaces.Infrastructure.Reports;
using ShiftOne.Shared.Requests.Reports;
using ShiftOne.Shared.Responses.Reports;

namespace ShiftOne.Infrastructure.Reports
{
    public class ReportExportService : IReportExportService
    {
        private readonly IExcelExportService _excelExportService;
        private readonly IPdfExportService _pdfExportService;

        public ReportExportService(IExcelExportService excelExportService, IPdfExportService pdfExportService)
        {
            _excelExportService = excelExportService;
            _pdfExportService = pdfExportService;
        }

        public Task<ReportExportFile> ExportAsync(ReportResult<object> report, string format, ReportRequest request)
        {
            var normalized = string.IsNullOrWhiteSpace(format) ? "xlsx" : format.Trim().ToLowerInvariant();
            return Task.FromResult(normalized switch
            {
                "xlsx" or "excel" => _excelExportService.Export(report, report.Columns, request),
                "pdf" => _pdfExportService.Export(report),
                _ => throw new NotSupportedException($"Report export format '{format}' is not supported.")
            });
        }
    }
}
