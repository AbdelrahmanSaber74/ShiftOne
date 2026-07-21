using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.Branches
{
    public class UpsertBranchRequest
    {
        public Guid? CompanyId { get; set; }
        [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
        [Required, MaxLength(50)] public string Code { get; set; } = string.Empty;
        [Required, MaxLength(300)] public string Address { get; set; } = string.Empty;
        [Range(-90, 90)] public decimal Latitude { get; set; }
        [Range(-180, 180)] public decimal Longitude { get; set; }
        [Range(1, 100000)] public int AllowedRadius { get; set; }
        public bool IsMainBranch { get; set; }
        public bool IsActive { get; set; } = true;
        public Guid? WorkScheduleId { get; set; }
    }
}
