using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.Roles
{
    public class UpsertRoleRequest
    {
        [Required, MaxLength(256)] public string Name { get; set; } = string.Empty;
        [MaxLength(500)] public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class AssignRolePermissionsRequest
    {
        public List<Guid> PermissionIds { get; set; } = new();
    }
}