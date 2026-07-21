using ShiftOne.Core.Entities.Companies;
using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.Subscriptions
{
    public class CompanySubscription : IAuditableEntity, IBaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public Guid PlanId { get; set; }
        public SubscriptionPlan Plan { get; set; } = null!;
        public DateTime StartsOn { get; set; }
        public DateTime? EndsOn { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
