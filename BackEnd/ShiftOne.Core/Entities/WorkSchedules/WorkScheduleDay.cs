using ShiftOne.Core.Entities.Contracts;

namespace ShiftOne.Core.Entities.WorkSchedules
{
    public class WorkScheduleDay : IBaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid WorkScheduleId { get; set; }
        public WorkSchedule WorkSchedule { get; set; } = null!;
        public DayOfWeek DayOfWeek { get; set; }
        public bool IsWorkingDay { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int LateGraceMinutes { get; set; }
        public int EarlyLeaveGraceMinutes { get; set; }
        public int MinimumWorkingMinutes { get; set; }
        public bool OvertimeEnabled { get; set; }
    }
}
