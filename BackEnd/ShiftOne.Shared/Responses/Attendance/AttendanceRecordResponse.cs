namespace ShiftOne.Shared.Responses.Attendance
{
    public class AttendanceRecordResponse
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public DateTime AttendanceDate { get; set; }
        public DateTime CheckInAt { get; set; }
        public DateTime? CheckOutAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public Guid? WorkScheduleId { get; set; }
        public string? WorkScheduleName { get; set; }
        public int? WorkedMinutes { get; set; }
        public int LateMinutes { get; set; }
        public int EarlyLeaveMinutes { get; set; }
        public int OvertimeMinutes { get; set; }
    }
}
