using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Requests.Plans
{
    public class UpsertSubscriptionPlanRequest
    {
        [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
        [MaxLength(500)] public string Description { get; set; } = string.Empty;
        [Range(0, double.MaxValue)] public decimal Price { get; set; }
        [Required, MaxLength(50)] public string BillingPeriod { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public int? MaxBranches { get; set; }
        public int? MaxEmployees { get; set; }
        public int? MaxHRUsers { get; set; }
        public int? MaxCompanyAdmins { get; set; }
    }
}
