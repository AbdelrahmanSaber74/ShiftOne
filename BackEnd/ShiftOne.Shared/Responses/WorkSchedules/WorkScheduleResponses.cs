namespace ShiftOne.Shared.Responses.WorkSchedules
{
    public class WorkScheduleDayResponse
    {
        public Guid Id { get; set; }
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsWorkingDay { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int LateGraceMinutes { get; set; }
        public int EarlyLeaveGraceMinutes { get; set; }
        public int MinimumWorkingMinutes { get; set; }
        public bool OvertimeEnabled { get; set; }
    }

    public class WorkScheduleResponse
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public Guid? BranchId { get; set; }
        public string? BranchName { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TimeZoneId { get; set; } = "UTC";
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; }
        public int WorkingDaysCount { get; set; }
        public DateTime CreatedOn { get; set; }
        public List<WorkScheduleDayResponse> Days { get; set; } = new();
    }
}
