using ShiftOne.Core.Entities.Companies;
using ShiftOne.Core.Entities.Contracts;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Entities.WorkSchedules;

namespace ShiftOne.Core.Entities.Branches
{
    public class Branch : IAuditableEntity, IBaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int AllowedRadius { get; set; }
        public bool IsMainBranch { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? WorkScheduleId { get; set; }
        public WorkSchedule? WorkSchedule { get; set; }

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
