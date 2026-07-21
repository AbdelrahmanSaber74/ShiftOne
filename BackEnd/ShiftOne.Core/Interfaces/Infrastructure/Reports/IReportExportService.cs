using ShiftOne.Shared.Requests.Reports;
using ShiftOne.Shared.Responses.Reports;

namespace ShiftOne.Core.Interfaces.Infrastructure.Reports
{
    public interface IReportExportService
    {
        Task<ReportExportFile> ExportAsync(ReportResult<object> report, string format, ReportRequest request);
    }
}
