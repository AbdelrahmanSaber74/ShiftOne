namespace ShiftOne.Shared.Responses.Reports
{
    public class EmployeeReportRow
    {
        public Guid Id { get; set; }
        public Guid? CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public Guid? BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public string EmployeeName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Roles { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime JoinedOn { get; set; }
        public bool HasBoundDevice { get; set; }
    }
}
