using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Attendance;
using ShiftOne.Core.Entities.Branches;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Attendance;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.Attendance;

namespace ShiftOne.Application.Services.Saas
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;
        private readonly IAttendanceCalculationService _attendanceCalculationService;

        public AttendanceService(IUnitOfWork unitOfWork, ITenantContext tenantContext, IAttendanceCalculationService attendanceCalculationService)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
            _attendanceCalculationService = attendanceCalculationService;
        }

        public async Task<GeneralResponse> CheckInAsync(AttendancePunchRequest request)
        {
            var validation = await ValidatePunchAsync(request);
            if (!validation.Success)
            {
                return validation.Response!;
            }

            var today = DateTime.UtcNow.Date;
            var existing = await _unitOfWork.Repository<AttendanceRecord>().CountAsync(Spec.For<AttendanceRecord>(record => record.EmployeeId == validation.User!.Id && record.AttendanceDate == today));
            if (existing > 0)
            {
                return GeneralResponse.BadRequest("Messages.AttendanceAlreadyCheckedIn");
            }

            var record = new AttendanceRecord
            {
                CompanyId = validation.User!.CompanyId!.Value,
                BranchId = validation.User.BranchId!.Value,
                EmployeeId = validation.User.Id,
                AttendanceDate = today,
                CheckInAt = DateTime.UtcNow,
                CheckInLatitude = request.Latitude,
                CheckInLongitude = request.Longitude,
                DeviceId = request.DeviceId.Trim()
            };
            ApplyCalculation(record, await _attendanceCalculationService.CalculateAsync(record, validation.Branch!));
            await _unitOfWork.Repository<AttendanceRecord>().AddAsync(record);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success", Map(record, validation.User, validation.Branch!));
        }

        public async Task<GeneralResponse> CheckOutAsync(AttendancePunchRequest request)
        {
            var validation = await ValidatePunchAsync(request);
            if (!validation.Success)
            {
                return validation.Response!;
            }

            var today = DateTime.UtcNow.Date;
            var record = (await _unitOfWork.Repository<AttendanceRecord>().GetAllAsync(Spec.For<AttendanceRecord>(r => r.EmployeeId == validation.User!.Id && r.AttendanceDate == today))).SingleOrDefault();
            if (record == null)
            {
                return GeneralResponse.BadRequest("Messages.CheckInRequired");
            }
            if (record.CheckOutAt.HasValue)
            {
                return GeneralResponse.BadRequest("Messages.AttendanceAlreadyCheckedOut");
            }

            record.CheckOutAt = DateTime.UtcNow;
            record.CheckOutLatitude = request.Latitude;
            record.CheckOutLongitude = request.Longitude;
            await _unitOfWork.Repository<AttendanceRecord>().UpdateAsync(record);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success", Map(record, validation.User!, validation.Branch!));
        }

        public async Task<GeneralResponse> GetMyHistoryAsync(int days = 3)
        {
            Guid userId;
            try
            {
                userId = _tenantContext.RequireUserId();
            }
            catch (UnauthorizedAccessException)
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            var safeDays = Math.Clamp(days, 1, 3);
            var fromDate = DateTime.UtcNow.Date.AddDays(-(safeDays - 1));

            var spec = Spec.ForChain<AttendanceRecord>(record =>
                    record.EmployeeId == userId && record.AttendanceDate >= fromDate,
                query => query.Include(record => record.Employee),
                query => query.Include(record => record.Branch));
            spec.ApplyOrderByDescending(record => record.AttendanceDate);

            var records = await _unitOfWork.Repository<AttendanceRecord>().GetAllAsync(spec);
            var data = records
                .OrderByDescending(record => record.AttendanceDate)
                .ThenByDescending(record => record.CheckInAt)
                .Select(record => Map(record, record.Employee, record.Branch))
                .ToList();

            return GeneralResponse.Ok("Messages.Success", data);
        }

        public async Task<GeneralResponse> GetAllAsync(PaginationRequest request, Guid? employeeId, Guid? branchId, DateTime? from, DateTime? to)
        {
            var companyId = _tenantContext.ResolveCompanyId(null);
            Specification<AttendanceRecord> spec;

            if (companyId.HasValue)
            {
                spec = Spec.ForChain<AttendanceRecord>(record => record.CompanyId == companyId.Value,
                    query => query.Include(record => record.Employee),
                    query => query.Include(record => record.Branch));
            }
            else if (_tenantContext.IsPlatformAdmin)
            {
                spec = Spec.ForChain<AttendanceRecord>(record => true,
                    query => query.Include(record => record.Employee),
                    query => query.Include(record => record.Branch));
            }
            else
            {
                return GeneralResponse.Unauthorized("Messages.CompanyContextRequired");
            }
            if (employeeId.HasValue) spec.AddCriteria(record => record.EmployeeId == employeeId.Value);
            if (branchId.HasValue) spec.AddCriteria(record => record.BranchId == branchId.Value);
            if (from.HasValue) spec.AddCriteria(record => record.AttendanceDate >= from.Value.Date);
            if (to.HasValue) spec.AddCriteria(record => record.AttendanceDate <= to.Value.Date);

            var countSpec = Spec.For<AttendanceRecord>(spec.Criteria);
            spec.ApplyOrderByDescending(record => record.CheckInAt);
            spec.ApplyPaging((request.Page - 1) * request.PageSize, request.PageSize);
            var data = (await _unitOfWork.Repository<AttendanceRecord>().GetAllAsync(spec)).Select(record => Map(record, record.Employee, record.Branch)).ToList();
            var count = await _unitOfWork.Repository<AttendanceRecord>().CountAsync(countSpec);
            return GeneralResponse.Ok("Messages.Success", data, request.Page, request.PageSize, count);
        }

        private async Task<PunchValidation> ValidatePunchAsync(AttendancePunchRequest request)
        {
            Guid userId;
            Guid companyId;
            try
            {
                userId = _tenantContext.RequireUserId();
                companyId = _tenantContext.RequireCompanyId();
            }
            catch (UnauthorizedAccessException)
            {
                return PunchValidation.Fail(GeneralResponse.Unauthorized("Messages.Unauthorized"));
            }

            var user = (await _unitOfWork.Repository<ApplicationUser>().GetAllAsync(Spec.For<ApplicationUser>(u => u.Id == userId))).SingleOrDefault();
            if (user == null || !user.IsActive)
            {
                return PunchValidation.Fail(GeneralResponse.Unauthorized("Messages.Unauthorized"));
            }

            if (!user.CompanyId.HasValue || user.CompanyId.Value != companyId)
            {
                return PunchValidation.Fail(GeneralResponse.Unauthorized("Messages.CompanyContextRequired"));
            }

            var activeDevice = (await _unitOfWork.Repository<EmployeeDevice>().GetAllAsync(Spec.For<EmployeeDevice>(device => device.EmployeeId == user.Id && device.IsActive))).SingleOrDefault();
            if (activeDevice == null || !string.Equals(activeDevice.DeviceId, request.DeviceId.Trim(), StringComparison.Ordinal))
            {
                return PunchValidation.Fail(GeneralResponse.Unauthorized("Messages.DeviceMismatch"));
            }

            if (!user.BranchId.HasValue)
            {
                return PunchValidation.Fail(GeneralResponse.BadRequest("Messages.BranchRequired"));
            }

            var branch = (await _unitOfWork.Repository<Branch>().GetAllAsync(Spec.For<Branch>(b => b.Id == user.BranchId.Value && b.CompanyId == user.CompanyId.Value && b.IsActive))).SingleOrDefault();
            if (branch == null)
            {
                return PunchValidation.Fail(GeneralResponse.BadRequest("Messages.BranchRequired"));
            }

            var distance = CalculateDistanceMeters((double)branch.Latitude, (double)branch.Longitude, (double)request.Latitude, (double)request.Longitude);
            if (distance > branch.AllowedRadius)
            {
                return PunchValidation.Fail(GeneralResponse.BadRequest("Messages.LocationOutOfRange"));
            }

            return PunchValidation.Ok(user, branch);
        }


        private static void ApplyCalculation(AttendanceRecord record, AttendanceCalculationResult result)
        {
            record.FinalStatus = result.Status;
            record.WorkScheduleId = result.WorkScheduleId;
            record.WorkScheduleName = result.WorkScheduleName;
            record.ScheduledStartTime = result.ScheduledStartTime;
            record.ScheduledEndTime = result.ScheduledEndTime;
            record.WorkedMinutes = result.WorkedMinutes;
            record.LateMinutes = result.LateMinutes;
            record.EarlyLeaveMinutes = result.EarlyLeaveMinutes;
            record.OvertimeMinutes = result.OvertimeMinutes;
        }
        private static double CalculateDistanceMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadiusMeters = 6371000;
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusMeters * c;
        }

        private static double ToRadians(double degrees) => degrees * Math.PI / 180;

        private static AttendanceRecordResponse Map(AttendanceRecord record, ApplicationUser employee, Branch branch) => new()
        {
            Id = record.Id,
            EmployeeId = record.EmployeeId,
            EmployeeName = $"{employee.FirstName} {employee.LastName}".Trim(),
            BranchId = record.BranchId,
            BranchName = branch.Name,
            AttendanceDate = record.AttendanceDate,
            CheckInAt = record.CheckInAt,
            CheckOutAt = record.CheckOutAt,
            Status = record.FinalStatus.ToString(),
            WorkScheduleId = record.WorkScheduleId,
            WorkScheduleName = record.WorkScheduleName,
            WorkedMinutes = record.WorkedMinutes,
            LateMinutes = record.LateMinutes,
            EarlyLeaveMinutes = record.EarlyLeaveMinutes,
            OvertimeMinutes = record.OvertimeMinutes
        };

        private sealed class PunchValidation
        {
            public bool Success { get; private set; }
            public GeneralResponse? Response { get; private set; }
            public ApplicationUser? User { get; private set; }
            public Branch? Branch { get; private set; }

            public static PunchValidation Fail(GeneralResponse response) => new() { Success = false, Response = response };
            public static PunchValidation Ok(ApplicationUser user, Branch branch) => new() { Success = true, User = user, Branch = branch };
        }
    }
}