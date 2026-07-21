using ShiftOne.Shared.Requests.Reports;
using ShiftOne.Shared.Responses.Reports;

namespace ShiftOne.Core.Interfaces.Infrastructure.Reports
{
    public interface IReportQueryProvider
    {
        Task<ReportResult<object>> QueryAsync(string reportKey, ReportRequest request);
    }
}
