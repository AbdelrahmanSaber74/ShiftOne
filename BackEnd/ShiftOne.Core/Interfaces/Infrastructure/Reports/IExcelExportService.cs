using ShiftOne.Shared.Requests.Reports;
using ShiftOne.Shared.Responses.Reports;

namespace ShiftOne.Core.Interfaces.Infrastructure.Reports
{
    public interface IExcelExportService
    {
        ReportExportFile Export(ReportResult<object> report, IReadOnlyList<ReportColumn> columns, ReportRequest request);
    }
}
