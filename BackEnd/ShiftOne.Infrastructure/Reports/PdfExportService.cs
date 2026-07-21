using ShiftOne.Core.Interfaces.Infrastructure.Reports;
using ShiftOne.Shared.Responses.Reports;

namespace ShiftOne.Infrastructure.Reports
{
    public class PdfExportService : IPdfExportService
    {
        public ReportExportFile Export(ReportResult<object> report)
        {
            return new ReportExportFile
            {
                Content = System.Text.Encoding.UTF8.GetBytes("PDF report export is not implemented yet."),
                ContentType = "text/plain",
                FileName = $"{report.ReportKey}-pdf-future-ready.txt"
            };
        }
    }
}
