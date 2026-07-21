using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.Identity.Base
{
    public class ApplicationRolePermission : IAuditableEntity, IBaseEntity<Guid>
    {
        public Guid RoleId { get; set; }
        public ApplicationRole Role { get; set; } = null!;

        public Guid PermissionId { get; set; }
        public ApplicationPermission Permission { get; set; } = null!;

        public Guid Id { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}


