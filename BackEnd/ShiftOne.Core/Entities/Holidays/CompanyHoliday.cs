using ShiftOne.Core.Entities.Companies;
using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.Holidays
{
    public class CompanyHoliday : IAuditableEntity, IBaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid? CompanyId { get; set; }
        public Company? Company { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsActive { get; set; } = true;
        public string? Notes { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
