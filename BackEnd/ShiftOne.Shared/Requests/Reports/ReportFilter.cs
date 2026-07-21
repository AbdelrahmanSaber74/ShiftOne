namespace ShiftOne.Shared.Requests.Reports
{
    public class ReportFilter
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? EmployeeId { get; set; }
        public Guid? ScheduleId { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string? Status { get; set; }
        public string? Role { get; set; }
        public string? Keyword { get; set; }
    }
}
