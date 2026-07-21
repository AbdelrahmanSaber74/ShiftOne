using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.Identity.Base
{
    public class EmployeeDevice : IAuditableEntity, IBaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public ApplicationUser Employee { get; set; } = null!;
        public string DeviceId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime BoundOn { get; set; }
        public DateTime? ResetOn { get; set; }
        public Guid? ResetBy { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
