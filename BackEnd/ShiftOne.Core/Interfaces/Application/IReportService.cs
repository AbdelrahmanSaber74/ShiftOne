using ShiftOne.Shared.Requests.Reports;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.Reports;

namespace ShiftOne.Core.Interfaces.Application
{
    public interface IReportService
    {
        Task<GeneralResponse> GetReportAsync(string reportKey, ReportRequest request);
        Task<ReportExportFile> ExportReportAsync(string reportKey, ReportRequest request, string format);
    }
}
