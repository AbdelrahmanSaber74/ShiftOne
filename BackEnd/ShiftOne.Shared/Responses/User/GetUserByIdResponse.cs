namespace ShiftOne.Shared.Responses.User
{
    public class GetUserByIdResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? ImagePath { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Email { get; set; } = string.Empty;
        public bool EmailConfirmed { get; set; }
        public bool PhoneConfirmed { get; set; }
        public bool IsProtected { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new ();
    }
}


