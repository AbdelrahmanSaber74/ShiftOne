using ShiftOne.Core.Entities.Branches;
using ShiftOne.Core.Entities.Companies;
using ShiftOne.Core.Entities.Contracts;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Entities.WorkSchedules;
using ShiftOne.Shared.Constants;

namespace ShiftOne.Core.Entities.Attendance
{
    public class AttendanceRecord : IAuditableEntity, IBaseEntity<Guid>
    {
        public Guid Id { get; set; }
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public Guid BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
        public Guid EmployeeId { get; set; }
        public ApplicationUser Employee { get; set; } = null!;
        public DateTime AttendanceDate { get; set; }
        public DateTime CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public decimal CheckInLatitude { get; set; }
        public decimal CheckInLongitude { get; set; }
        public decimal? CheckOutLatitude { get; set; }
        public decimal? CheckOutLongitude { get; set; }
        public string DeviceId { get; set; } = string.Empty;
        public AttendanceStatus FinalStatus { get; set; } = AttendanceStatus.MissingCheckOut;
        public Guid? WorkScheduleId { get; set; }
        public WorkSchedule? WorkSchedule { get; set; }
        public string? WorkScheduleName { get; set; }
        public TimeSpan? ScheduledStartTime { get; set; }
        public TimeSpan? ScheduledEndTime { get; set; }
        public int? WorkedMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public int OvertimeMinutes { get; set; }

        public Guid CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }
}
