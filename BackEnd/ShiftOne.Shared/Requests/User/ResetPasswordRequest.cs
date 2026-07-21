using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.User
{
    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Reset token is required.")]
        public string resetToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(8, ErrorMessage = "Passwords must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)[A-Za-z\d@$!%*?&]{8,}$", ErrorMessage = "Passwords must have at least one uppercase ('A'-'Z'), at least one digit ('0'-'9'), and be at least 8 characters long.")]
        [MaxLength(50, ErrorMessage = "Password cannot exceed 50 characters.")]
        public string newPassword { get; set; } = string.Empty;
    }
}


