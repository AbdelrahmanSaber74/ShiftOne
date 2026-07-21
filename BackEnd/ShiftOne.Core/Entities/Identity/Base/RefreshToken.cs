using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.Identity.Base
{
    public class RefreshToken : IBaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public string TokenHash { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedByIp { get; set; } = string.Empty;
        public DateTime? RevokedOn { get; set; }
        public string? RevokedByIp { get; set; }
        public string? ReplacedByTokenHash { get; set; }
        public string? ReasonRevoked { get; set; }
        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; } = null!;

        public bool IsActive => !IsRevoked && RevokedOn == null && ExpiryDate > DateTime.UtcNow;
    }
}


