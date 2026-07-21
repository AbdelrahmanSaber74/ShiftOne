using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.Identity.Base
{
    public class ApplicationPermission : IAuditableEntity, IBaseEntity<Guid>
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public ICollection<ApplicationRolePermission> RolePermissions { get; set; } = new List<ApplicationRolePermission>();

        public Guid Id { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}


