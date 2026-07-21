using ShiftOne.Core.Entities.Attendance;
using ShiftOne.Core.Entities.Branches;
using ShiftOne.Core.Entities.WorkSchedules;
using ShiftOne.Shared.Constants;

namespace ShiftOne.Core.Interfaces.Application
{
    public class AttendanceCalculationResult
    {
        public AttendanceStatus Status { get; set; }
        public Guid? WorkScheduleId { get; set; }
        public string? WorkScheduleName { get; set; }
        public TimeSpan? ScheduledStartTime { get; set; }
        public TimeSpan? ScheduledEndTime { get; set; }
        public int? WorkedMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public int OvertimeMinutes { get; set; }
    }

    public interface IAttendanceCalculationService
    {
        Task<AttendanceCalculationResult> CalculateAsync(AttendanceRecord record, Branch branch);
    }

    public interface IAttendanceStatusResolver
    {
        AttendanceCalculationResult Resolve(AttendanceRecord record, WorkSchedule? schedule, WorkScheduleDay? scheduleDay, TimeZoneInfo timeZone);
    }
}
