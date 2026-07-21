using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.User
{
    public class EditProfileRequest
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string? firstName { get; set; }

        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string? lastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public string? email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        public string? phone { get; set; }

        public IFormFile? Picture { get; set; }
    }
}
