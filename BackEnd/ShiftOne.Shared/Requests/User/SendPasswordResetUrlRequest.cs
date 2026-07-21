using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.User
{
    public class SendPasswordResetUrlRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string email { get; set; } = string.Empty;

        [Required(ErrorMessage = "restLink is required")]
        public string restLink { get; set; } = string.Empty;
    }
}
