namespace ShiftOne.Shared.Responses.Reports
{
    public class CompanyReportRow
    {
        public Guid Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public int BranchesCount { get; set; }
        public int EmployeesCount { get; set; }
        public string SubscriptionStatus { get; set; } = string.Empty;
        public DateTime? ExpirationDate { get; set; }
    }
}
