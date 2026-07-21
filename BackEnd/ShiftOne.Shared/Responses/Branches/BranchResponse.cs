namespace ShiftOne.Shared.Responses.Branches
{
    public class BranchResponse
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public int AllowedRadius { get; set; }
        public bool IsMainBranch { get; set; }
        public bool IsActive { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public string? WorkScheduleName { get; set; }
        public bool UsesCompanyDefaultSchedule => !WorkScheduleId.HasValue;
    }
}
