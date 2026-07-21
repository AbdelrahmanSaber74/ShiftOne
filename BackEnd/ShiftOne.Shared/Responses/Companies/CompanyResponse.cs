namespace ShiftOne.Shared.Responses.Companies
{
    public class CompanyResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsActive { get; set; }
        public string? CurrentPlanName { get; set; }
        public Guid? CurrentPlanId { get; set; }
    }
}
