using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.User
{
    public class AdminActivateUserEmailRequest : IValidatableObject
    {
        [Required(ErrorMessage = "User id is required.")]
        public Guid userId { get; set; }

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
