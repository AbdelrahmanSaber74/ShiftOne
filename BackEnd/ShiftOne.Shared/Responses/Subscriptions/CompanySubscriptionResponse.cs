namespace ShiftOne.Shared.Responses.Subscriptions
{
    public class CompanySubscriptionResponse
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartsOn { get; set; }
        public DateTime? EndsOn { get; set; }
        public bool IsActive { get; set; }
    }
}
