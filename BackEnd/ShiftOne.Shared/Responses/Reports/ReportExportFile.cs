namespace ShiftOne.Shared.Responses.Reports
{
    public class ReportExportFile
    {
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string ContentType { get; set; } = "application/octet-stream";
        public string FileName { get; set; } = "report.bin";
    }
}
