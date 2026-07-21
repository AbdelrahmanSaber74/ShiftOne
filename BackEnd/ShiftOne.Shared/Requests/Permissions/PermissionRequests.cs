using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.Permissions
{
    public class UpsertPermissionRequest
    {
        [Required, MaxLength(256)] public string Name { get; set; } = string.Empty;
        [MaxLength(500)] public string Description { get; set; } = string.Empty;
    }
}