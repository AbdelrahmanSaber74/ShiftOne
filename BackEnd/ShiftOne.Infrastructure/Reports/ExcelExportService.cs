using ClosedXML.Excel;
using ShiftOne.Core.Interfaces.Infrastructure.Reports;
using ShiftOne.Shared.Requests.Reports;
using ShiftOne.Shared.Responses.Reports;
using System.Reflection;

namespace ShiftOne.Infrastructure.Reports
{
    public class ExcelExportService : IExcelExportService
    {
        private const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

        public ReportExportFile Export(ReportResult<object> report, IReadOnlyList<ReportColumn> columns, ReportRequest request)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(SafeSheetName(report.Title));
            var row = 1;

            worksheet.Cell(row, 1).Value = report.Title;
            worksheet.Range(row, 1, row, Math.Max(columns.Count, 1)).Merge().Style.Font.SetBold().Font.SetFontSize(16);
            row++;
            worksheet.Cell(row, 1).Value = "Generated On";
            worksheet.Cell(row, 2).Value = report.GeneratedOn;
            worksheet.Cell(row, 2).Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
            row += 2;

            worksheet.Cell(row, 1).Value = "Filters Applied";
            worksheet.Cell(row, 1).Style.Font.SetBold();
            row++;
            if (report.AppliedFilters.Count == 0)
            {
                worksheet.Cell(row, 1).Value = "None";
                row++;
            }
            else
            {
                foreach (var filter in report.AppliedFilters)
                {
                    worksheet.Cell(row, 1).Value = filter.Key;
                    worksheet.Cell(row, 2).Value = filter.Value;
                    row++;
                }
            }

            row++;
            var headerRow = row;
            for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
            {
                var cell = worksheet.Cell(headerRow, columnIndex + 1);
                cell.Value = columns[columnIndex].Header;
                cell.Style.Font.SetBold();
                cell.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#F7FAFC"));
                cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            }

            row++;
            foreach (var item in report.Rows)
            {
                for (var columnIndex = 0; columnIndex < columns.Count; columnIndex++)
                {
                    var value = GetValue(item, columns[columnIndex].Key);
                    var cell = worksheet.Cell(row, columnIndex + 1);
                    SetCellValue(cell, value, columns[columnIndex].Key);
                }
                row++;
            }

            if (report.Rows.Count > 0)
            {
                var tableRange = worksheet.Range(headerRow, 1, row - 1, columns.Count);
                tableRange.CreateTable();
            }

            worksheet.Columns().AdjustToContents();
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return new ReportExportFile
            {
                Content = stream.ToArray(),
                ContentType = ContentType,
                FileName = $"{report.ReportKey}-{DateTime.UtcNow:yyyyMMddHHmmss}.xlsx"
            };
        }

        private static string SafeSheetName(string title)
        {
            var invalid = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            var safe = invalid.Aggregate(title, (current, c) => current.Replace(c, '-'));
            return safe.Length > 31 ? safe[..31] : safe;
        }

        private static object? GetValue(object item, string key)
        {
            var property = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .FirstOrDefault(info => string.Equals(info.Name, key, StringComparison.OrdinalIgnoreCase));
            return property?.GetValue(item);
        }

        private static void SetCellValue(IXLCell cell, object? value, string columnKey)
        {
            switch (value)
            {
                case null:
                    cell.Value = string.Empty;
                    break;
                case DateTime date:
                    cell.Value = date;
                    cell.Style.DateFormat.Format = "yyyy-mm-dd hh:mm";
                    break;
                case bool boolean:
                    cell.Value = boolean ? "Yes" : "No";
                    break;
                case decimal dec:
                    cell.Value = dec;
                    break;
                case double dbl:
                    cell.Value = dbl;
                    break;
                case int integer:
                    cell.Value = integer;
                    break;
                default:
                    cell.Value = value.ToString() ?? string.Empty;
                    break;
            }
        }
    }
}
