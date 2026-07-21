using ShiftOne.Core.Entities.Companies;
using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.WorkSchedules
{
    public class WorkSchedule : IAuditableEntity, IBaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TimeZoneId { get; set; } = "Arab Standard Time";
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<WorkScheduleDay> Days { get; set; } = new List<WorkScheduleDay>();

        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
