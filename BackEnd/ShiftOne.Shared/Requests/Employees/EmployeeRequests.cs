using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.Employees
{
    public class CreateEmployeeRequest
    {
        [Required, MaxLength(50)] public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string LastName { get; set; } = string.Empty;
        [EmailAddress] public string? Email { get; set; }
        [MaxLength(30)] public string? PhoneNumber { get; set; }
        public Guid? BranchId { get; set; }
        [Required] public string Role { get; set; } = string.Empty;
        [Required, MinLength(8), MaxLength(50)] public string Password { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }

    public class UpdateEmployeeRequest
    {
        [Required, MaxLength(50)] public string FirstName { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string LastName { get; set; } = string.Empty;
        [EmailAddress] public string? Email { get; set; }
        [MaxLength(30)] public string? PhoneNumber { get; set; }
        public Guid? BranchId { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

