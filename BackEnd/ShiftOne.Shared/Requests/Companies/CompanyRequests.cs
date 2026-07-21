using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.Companies
{
    public class CreateCompanyRequest
    {
        [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string Code { get; set; } = string.Empty;
        [EmailAddress] public string? Email { get; set; }
        [MaxLength(30)] public string? PhoneNumber { get; set; }
        [MaxLength(300)] public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? PlanId { get; set; }
        public string? AdminFirstName { get; set; }
        public string? AdminLastName { get; set; }
        public string? AdminEmail { get; set; }
        public string? AdminPassword { get; set; }
    }

    public class UpdateCompanyRequest
    {
        [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string Code { get; set; } = string.Empty;
        [EmailAddress] public string? Email { get; set; }
        [MaxLength(30)] public string? PhoneNumber { get; set; }
        [MaxLength(300)] public string? Address { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? PlanId { get; set; }
    }
}
