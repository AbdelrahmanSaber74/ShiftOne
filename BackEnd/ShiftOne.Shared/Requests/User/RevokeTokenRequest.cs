using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.User
{
    public class RevokeTokenRequest
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
