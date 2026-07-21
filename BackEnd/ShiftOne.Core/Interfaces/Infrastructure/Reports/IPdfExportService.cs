using ShiftOne.Shared.Responses.Reports;

namespace ShiftOne.Core.Interfaces.Infrastructure.Reports
{
    public interface IPdfExportService
    {
        ReportExportFile Export(ReportResult<object> report);
    }
}
