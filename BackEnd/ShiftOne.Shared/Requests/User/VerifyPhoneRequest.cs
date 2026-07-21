using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.User
{
    public class VerifyPhoneRequest
    {
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [StringLength(15, MinimumLength = 7, ErrorMessage = "Phone number must be between 7 and 15 digits")]
        public string phone { get; set; } = string.Empty;
        [Required(ErrorMessage = "Verification code is required.")]
        [StringLength(6, ErrorMessage = "Verification code cannot exceed 6 characters.")]
        public string verificationCode { get; set; } = string.Empty;
    }
}


