using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.Subscriptions
{
    public class AssignCompanySubscriptionRequest
    {
        [Required] public Guid CompanyId { get; set; }
        [Required] public Guid PlanId { get; set; }
        public DateTime StartsOn { get; set; } = DateTime.UtcNow;
        public DateTime? EndsOn { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
