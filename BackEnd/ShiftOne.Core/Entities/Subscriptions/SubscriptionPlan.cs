using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.Subscriptions
{
    public class SubscriptionPlan : IAuditableEntity, IBaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string BillingPeriod { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int? MaxBranches { get; set; }
        public int? MaxEmployees { get; set; }
        public int? MaxHRUsers { get; set; }
        public int? MaxCompanyAdmins { get; set; }

        public ICollection<CompanySubscription> CompanySubscriptions { get; set; } = new List<CompanySubscription>();

        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
