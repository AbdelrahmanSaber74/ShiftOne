using ShiftOne.Core.Entities.Branches;
using ShiftOne.Core.Entities.Contracts;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Entities.Subscriptions;
using ShiftOne.Core.Entities.WorkSchedules;

namespace ShiftOne.Core.Entities.Companies
{
    public class Company : IAuditableEntity, IBaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
        public ICollection<Branch> Branches { get; set; } = new List<Branch>();
        public ICollection<CompanySubscription> Subscriptions { get; set; } = new List<CompanySubscription>();
        public ICollection<WorkSchedule> WorkSchedules { get; set; } = new List<WorkSchedule>();

        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
