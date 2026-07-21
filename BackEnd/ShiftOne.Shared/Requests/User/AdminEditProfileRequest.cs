using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.User
{
    public class AdminEditProfileRequest
    {
        [Required(ErrorMessage = "Id is required")]
        public Guid Id { get; set; }
        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First name must be between 2 and 50 characters")]
        public string firstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        public string lastName { get; set; } = string.Empty;
        public IFormFile? Picture { get; set; }
    }
}

