namespace ShiftOne.Shared.Requests.WorkSchedules
{
    public class WorkScheduleDayRequest
    {
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsWorkingDay { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int LateGraceMinutes { get; set; }
        public int EarlyLeaveGraceMinutes { get; set; }
        public int MinimumWorkingMinutes { get; set; }
        public bool OvertimeEnabled { get; set; }
    }

    public class UpsertWorkScheduleRequest
    {
        public Guid? CompanyId { get; set; }
        public Guid? BranchId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string TimeZoneId { get; set; } = "UTC";
        public bool IsDefault { get; set; }
        public bool IsActive { get; set; } = true;
        public List<WorkScheduleDayRequest> Days { get; set; } = new();
    }

    public class AssignBranchScheduleRequest
    {
        public Guid? WorkScheduleId { get; set; }
    }
}
