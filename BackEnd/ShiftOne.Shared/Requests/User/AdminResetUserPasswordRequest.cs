using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.User
{
    public class AdminResetUserPasswordRequest : IValidatableObject
    {
        [Required(ErrorMessage = "User id is required.")]
        public Guid userId { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Passwords must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Passwords must have at least one uppercase ('A'-'Z'), at least one digit ('0'-'9'), and be at least 8 characters long.")]
        [MaxLength(50, ErrorMessage = "Password cannot exceed 50 characters.")]
        public string newPassword { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (userId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "User id is required.",
                    new[] { nameof(userId) });
            }
        }
    }
}
