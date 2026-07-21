namespace ShiftOne.Shared.Responses.Plans
{
    public class SubscriptionPlanResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string BillingPeriod { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? MaxBranches { get; set; }
        public int? MaxEmployees { get; set; }
        public int? MaxHRUsers { get; set; }
        public int? MaxCompanyAdmins { get; set; }
    }
}
