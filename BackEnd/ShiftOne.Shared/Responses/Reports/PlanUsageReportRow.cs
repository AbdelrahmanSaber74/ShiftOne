namespace ShiftOne.Shared.Responses.Reports
{
    public class PlanUsageReportRow
    {
        public Guid Id { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public int CompaniesCount { get; set; }
        public int EmployeesCount { get; set; }
        public int BranchesCount { get; set; }
        public decimal AverageUsage { get; set; }
    }
}
