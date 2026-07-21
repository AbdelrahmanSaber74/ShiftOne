namespace ShiftOne.Shared.Responses.Roles
{
    public class RoleResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsSystemRole { get; set; }
        public bool IsProtected { get; set; }
        public List<string> Permissions { get; set; } = new();
        public DateTime CreatedOn { get; set; }
    }
}