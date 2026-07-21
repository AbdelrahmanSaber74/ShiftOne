using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.User
{
    public class VerifyEmailRequest
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [MaxLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Verification code is required.")]
        [StringLength(6, ErrorMessage = "Verification code cannot exceed 6 characters.")]
        public string verificationCode { get; set; } = string.Empty;
    }
}


