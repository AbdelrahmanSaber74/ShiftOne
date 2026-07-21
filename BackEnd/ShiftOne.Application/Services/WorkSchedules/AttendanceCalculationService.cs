using ShiftOne.Core.Entities.Attendance;
using ShiftOne.Core.Entities.Branches;
using ShiftOne.Core.Entities.WorkSchedules;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Constants;

namespace ShiftOne.Application.Services.WorkSchedules
{
    public class AttendanceCalculationService : IAttendanceCalculationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAttendanceStatusResolver _resolver;

        public AttendanceCalculationService(IUnitOfWork unitOfWork, IAttendanceStatusResolver resolver)
        {
            _unitOfWork = unitOfWork;
            _resolver = resolver;
        }

        public async Task<AttendanceCalculationResult> CalculateAsync(AttendanceRecord record, Branch branch)
        {
            var schedule = await ResolveScheduleAsync(branch);
            var timeZone = ResolveTimeZone(schedule?.TimeZoneId);
            var localDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(record.CheckInAt, DateTimeKind.Utc), timeZone).Date;
            var scheduleDay = schedule?.Days.SingleOrDefault(day => day.DayOfWeek == localDate.DayOfWeek);
            return _resolver.Resolve(record, schedule, scheduleDay, timeZone);
        }

        private async Task<WorkSchedule?> ResolveScheduleAsync(Branch branch)
        {
            if (branch.WorkScheduleId.HasValue)
            {
                var branchSchedule = (await _unitOfWork.Repository<WorkSchedule>().GetAllAsync(
                    Spec.ForChain<WorkSchedule>(x => x.Id == branch.WorkScheduleId.Value && x.IsActive, query => Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(query, x => x.Days))))
                    .SingleOrDefault();
                if (branchSchedule != null) return branchSchedule;
            }

            return (await _unitOfWork.Repository<WorkSchedule>().GetAllAsync(
                    Spec.ForChain<WorkSchedule>(x => x.CompanyId == branch.CompanyId && x.IsDefault && x.IsActive, query => Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(query, x => x.Days))))
                .OrderByDescending(x => x.CreatedOn)
                .FirstOrDefault();
        }

        private static TimeZoneInfo ResolveTimeZone(string? timeZoneId)
        {
            foreach (var candidate in new[] { string.IsNullOrWhiteSpace(timeZoneId) ? "Arab Standard Time" : timeZoneId.Trim(), "Asia/Riyadh", "UTC" })
            {
                try { return TimeZoneInfo.FindSystemTimeZoneById(candidate); }
                catch { }
            }
            return TimeZoneInfo.Utc;
        }
    }

    public class AttendanceStatusResolver : IAttendanceStatusResolver
    {
        public AttendanceCalculationResult Resolve(AttendanceRecord record, WorkSchedule? schedule, WorkScheduleDay? scheduleDay, TimeZoneInfo timeZone)
        {
            var result = new AttendanceCalculationResult
            {
                WorkScheduleId = schedule?.Id,
                WorkScheduleName = schedule?.Name,
                ScheduledStartTime = scheduleDay?.StartTime,
                ScheduledEndTime = scheduleDay?.EndTime,
                Status = record.CheckOutAt.HasValue ? AttendanceStatus.Present : AttendanceStatus.MissingCheckOut
            };

            if (schedule == null || scheduleDay == null || !scheduleDay.IsWorkingDay || !scheduleDay.StartTime.HasValue || !scheduleDay.EndTime.HasValue)
            {
                result.Status = AttendanceStatus.OutsideSchedule;
                return result;
            }

            var checkInLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(record.CheckInAt, DateTimeKind.Utc), timeZone);
            var checkOutLocal = record.CheckOutAt.HasValue ? TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(record.CheckOutAt.Value, DateTimeKind.Utc), timeZone) : (DateTime?)null;
            var scheduledStart = checkInLocal.Date.Add(scheduleDay.StartTime.Value);
            var scheduledEnd = checkInLocal.Date.Add(scheduleDay.EndTime.Value);

            result.LateMinutes = Math.Max(0, (int)Math.Floor((checkInLocal - scheduledStart.AddMinutes(scheduleDay.LateGraceMinutes)).TotalMinutes));
            if (result.LateMinutes > 0) result.Status = AttendanceStatus.Late;
            else if (checkInLocal < scheduledStart) result.Status = AttendanceStatus.EarlyArrival;
            else result.Status = record.CheckOutAt.HasValue ? AttendanceStatus.Present : AttendanceStatus.MissingCheckOut;

            if (!checkOutLocal.HasValue) return result;

            result.WorkedMinutes = Math.Max(0, (int)Math.Floor((checkOutLocal.Value - checkInLocal).TotalMinutes));
            result.EarlyLeaveMinutes = Math.Max(0, (int)Math.Floor((scheduledEnd.AddMinutes(-scheduleDay.EarlyLeaveGraceMinutes) - checkOutLocal.Value).TotalMinutes));
            result.OvertimeMinutes = scheduleDay.OvertimeEnabled ? Math.Max(0, (int)Math.Floor((checkOutLocal.Value - scheduledEnd).TotalMinutes)) : 0;

            if (result.EarlyLeaveMinutes > 0) result.Status = AttendanceStatus.EarlyLeave;
            else if (result.OvertimeMinutes > 0) result.Status = AttendanceStatus.Overtime;
            else if (result.LateMinutes > 0) result.Status = AttendanceStatus.Late;
            else if (checkInLocal < scheduledStart) result.Status = AttendanceStatus.EarlyArrival;
            else result.Status = AttendanceStatus.Present;

            return result;
        }
    }
}
