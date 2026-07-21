using Microsoft.AspNetCore.Identity;
using ShiftOne.Core.Entities.Branches;
using ShiftOne.Core.Entities.Companies;
using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.Identity.Base
{
    public class ApplicationUser : IdentityUser<Guid>, IAuditableEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public string? ImagePath { get; set; }
        public Guid? CompanyId { get; set; }
        public Company? Company { get; set; }
        public Guid? BranchId { get; set; }
        public Branch? Branch { get; set; }


        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<EmployeeDevice> EmployeeDevices { get; set; } = new List<EmployeeDevice>();
    }
}



