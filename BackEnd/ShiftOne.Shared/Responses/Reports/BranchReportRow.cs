namespace ShiftOne.Shared.Responses.Reports
{
    public class BranchReportRow
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public int EmployeesCount { get; set; }
        public int AttendanceToday { get; set; }
        public string GeoFenceStatus { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
