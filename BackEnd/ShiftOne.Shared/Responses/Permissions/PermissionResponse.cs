namespace ShiftOne.Shared.Responses.Permissions
{
    public class PermissionResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsSystemPermission { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}