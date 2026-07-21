using Microsoft.AspNetCore.Identity;
using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.Identity.Base
{
    public class ApplicationRole : IdentityRole<Guid>, IAuditableEntity
    {
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }

        public ICollection<ApplicationRolePermission> RolePermissions { get; set; } = new List<ApplicationRolePermission>();

        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}


