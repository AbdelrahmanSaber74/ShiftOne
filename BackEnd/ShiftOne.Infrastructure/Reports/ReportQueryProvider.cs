using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.WorkSchedules;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Core.Interfaces.Infrastructure.Reports;
using ShiftOne.Infrastructure.Persistence;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests.Reports;
using ShiftOne.Shared.Responses.Reports;

namespace ShiftOne.Infrastructure.Reports
{
    public class ReportQueryProvider : IReportQueryProvider
    {
        private readonly ApplicationDbContext _db;
        private readonly ITenantContext _tenantContext;

        public ReportQueryProvider(ApplicationDbContext db, ITenantContext tenantContext)
        {
            _db = db;
            _tenantContext = tenantContext;
        }

        public Task<ReportResult<object>> QueryAsync(string reportKey, ReportRequest request) => reportKey switch
        {
            ReportDefinitions.Attendance => AttendanceAsync(request),
            ReportDefinitions.Employees => EmployeesAsync(request),
            ReportDefinitions.Companies => CompaniesAsync(request),
            ReportDefinitions.Branches => BranchesAsync(request),
            ReportDefinitions.Subscriptions => SubscriptionsAsync(request),
            ReportDefinitions.PlanUsage => PlanUsageAsync(request),
            _ => throw new KeyNotFoundException(reportKey)
        };

        private Guid? TenantCompanyId(string reportKey, ReportRequest request)
        {
            if (_tenantContext.IsPlatformAdmin)
            {
                if (reportKey == ReportDefinitions.Companies || 
                    reportKey == ReportDefinitions.Subscriptions || 
                    reportKey == ReportDefinitions.PlanUsage)
                {
                    return request.CompanyId;
                }
                return request.CompanyId ?? _tenantContext.CompanyId;
            }
            return _tenantContext.CompanyId;
        }

        private async Task<ReportResult<object>> AttendanceAsync(ReportRequest request)
        {
            var status = NormalizeStatus(request.Status);
            var singleDay = TryGetSingleDay(request, out var selectedDay);
            var syntheticStatus = status is "absent" or "dayoff" or "holiday";
            var companyId = TenantCompanyId(ReportDefinitions.Attendance, request);
            var query = _db.AttendanceRecords.AsNoTracking()
                .Select(record => new AttendanceReportRow
                {
                    Id = record.Id,
                    CompanyId = record.CompanyId,
                    CompanyName = record.Company.Name,
                    BranchId = record.BranchId,
                    BranchName = record.Branch.Name,
                    EmployeeId = record.EmployeeId,
                    EmployeeName = (record.Employee.FirstName + " " + record.Employee.LastName).Trim(),
                    AttendanceDate = record.AttendanceDate,
                    Status = record.FinalStatus.ToString(),
                    WorkScheduleId = record.WorkScheduleId,
                    WorkScheduleName = record.WorkScheduleName,
                    HolidayName = null,
                    ScheduledStartTime = record.ScheduledStartTime,
                    ScheduledEndTime = record.ScheduledEndTime,
                    CheckInAt = record.CheckInAt,
                    CheckOutAt = record.CheckOutAt,
                    DeviceId = record.DeviceId,
                    WorkedMinutes = record.WorkedMinutes,
                    LateMinutes = record.LateMinutes,
                    EarlyLeaveMinutes = record.EarlyLeaveMinutes,
                    OvertimeMinutes = record.OvertimeMinutes
                });

            if (companyId.HasValue) query = query.Where(x => x.CompanyId == companyId.Value);
            if (request.BranchId.HasValue) query = query.Where(x => x.BranchId == request.BranchId.Value);
            if (request.EmployeeId.HasValue) query = query.Where(x => x.EmployeeId == request.EmployeeId.Value);
            if (request.ScheduleId.HasValue) query = query.Where(x => x.WorkScheduleId == request.ScheduleId.Value);
            if (request.From.HasValue) query = query.Where(x => x.AttendanceDate >= request.From.Value.Date);
            if (request.To.HasValue) query = query.Where(x => x.AttendanceDate <= request.To.Value.Date);
            if (!string.IsNullOrWhiteSpace(status) && !syntheticStatus) query = query.Where(x => x.Status.ToLower() == status);
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(x => x.CompanyName.Contains(keyword) || x.BranchName.Contains(keyword) || x.EmployeeName.Contains(keyword) || x.DeviceId.Contains(keyword));
            }

            if (singleDay && (string.IsNullOrWhiteSpace(status) || syntheticStatus))
            {
                var rows = syntheticStatus ? new List<AttendanceReportRow>() : await query.ToListAsync();
                rows.AddRange(await BuildSyntheticAttendanceRowsAsync(request, selectedDay, status));
                return Page(ReportDefinitions.Attendance, SortAttendance(rows.AsQueryable(), request), request, x => x.AttendanceDate, true);
            }

            return await PageAsync(ReportDefinitions.Attendance, SortAttendance(query, request), request);
        }

        private async Task<List<AttendanceReportRow>> BuildSyntheticAttendanceRowsAsync(ReportRequest request, DateTime selectedDay, string? normalizedStatus)
        {
            var companyId = TenantCompanyId(ReportDefinitions.Attendance, request);
            var attendanceQuery = _db.AttendanceRecords.AsNoTracking().Where(a => a.AttendanceDate == selectedDay.Date);
            if (companyId.HasValue) attendanceQuery = attendanceQuery.Where(a => a.CompanyId == companyId.Value);
            if (request.BranchId.HasValue) attendanceQuery = attendanceQuery.Where(a => a.BranchId == request.BranchId.Value);
            if (request.EmployeeId.HasValue) attendanceQuery = attendanceQuery.Where(a => a.EmployeeId == request.EmployeeId.Value);
            var attendanceEmployeeIds = await attendanceQuery.Select(a => a.EmployeeId).ToListAsync();

            var usersQuery = _db.ApplicationUsers.AsNoTracking()
                .Include(user => user.Company)
                .Include(user => user.Branch)
                .Where(user => user.IsActive && user.CompanyId.HasValue && !attendanceEmployeeIds.Contains(user.Id));
            if (companyId.HasValue) usersQuery = usersQuery.Where(user => user.CompanyId == companyId.Value);
            if (request.BranchId.HasValue) usersQuery = usersQuery.Where(user => user.BranchId == request.BranchId.Value);
            if (request.EmployeeId.HasValue) usersQuery = usersQuery.Where(user => user.Id == request.EmployeeId.Value);
            var users = await usersQuery.ToListAsync();
            if (users.Count == 0) return new List<AttendanceReportRow>();

            var companyIds = users.Select(user => user.CompanyId!.Value).Distinct().ToList();
            var branchScheduleIds = users.Where(user => user.Branch?.WorkScheduleId != null).Select(user => user.Branch!.WorkScheduleId!.Value).Distinct().ToList();
            var schedules = await _db.WorkSchedules.AsNoTracking()
                .Include(schedule => schedule.Days)
                .Where(schedule => schedule.IsActive && (companyIds.Contains(schedule.CompanyId) || branchScheduleIds.Contains(schedule.Id)))
                .ToListAsync();
            var holidays = await _db.CompanyHolidays.AsNoTracking()
                .Where(holiday => holiday.IsActive && holiday.Date == selectedDay.Date && (holiday.CompanyId == null || companyIds.Contains(holiday.CompanyId.Value)))
                .ToListAsync();
            var globalHoliday = holidays.FirstOrDefault(holiday => holiday.CompanyId == null);
            var rows = new List<AttendanceReportRow>();
            var keyword = request.Keyword?.Trim();

            foreach (var user in users)
            {
                var holiday = holidays.FirstOrDefault(item => item.CompanyId == user.CompanyId) ?? globalHoliday;
                var schedule = user.Branch?.WorkScheduleId is Guid branchScheduleId
                    ? schedules.FirstOrDefault(item => item.Id == branchScheduleId)
                    : null;
                schedule ??= schedules.FirstOrDefault(item => item.CompanyId == user.CompanyId && item.IsDefault);
                if (request.ScheduleId.HasValue && schedule?.Id != request.ScheduleId.Value) continue;

                var scheduleDay = schedule?.Days.SingleOrDefault(day => day.DayOfWeek == selectedDay.DayOfWeek);
                var status = ResolveSyntheticStatus(selectedDay, holiday != null, scheduleDay);
                if (!string.IsNullOrWhiteSpace(normalizedStatus) && NormalizeStatus(status.ToString()) != normalizedStatus) continue;

                var row = new AttendanceReportRow
                {
                    Id = user.Id,
                    CompanyId = user.CompanyId,
                    CompanyName = user.Company?.Name ?? string.Empty,
                    BranchId = user.BranchId,
                    BranchName = user.Branch?.Name ?? string.Empty,
                    EmployeeId = user.Id,
                    EmployeeName = (user.FirstName + " " + user.LastName).Trim(),
                    AttendanceDate = selectedDay.Date,
                    Status = status.ToString(),
                    WorkScheduleId = schedule?.Id,
                    WorkScheduleName = schedule?.Name,
                    HolidayName = holiday?.Name,
                    ScheduledStartTime = scheduleDay?.StartTime,
                    ScheduledEndTime = scheduleDay?.EndTime,
                    CheckInAt = null,
                    CheckOutAt = null,
                    DeviceId = string.Empty
                };

                if (!string.IsNullOrWhiteSpace(keyword) &&
                    !row.CompanyName.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    !row.BranchName.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    !row.EmployeeName.Contains(keyword, StringComparison.OrdinalIgnoreCase) &&
                    !(row.HolidayName?.Contains(keyword, StringComparison.OrdinalIgnoreCase) ?? false))
                {
                    continue;
                }

                rows.Add(row);
            }

            return rows;
        }

        private static AttendanceStatus ResolveSyntheticStatus(DateTime selectedDay, bool isHoliday, WorkScheduleDay? scheduleDay)
        {
            if (isHoliday) return AttendanceStatus.Holiday;
            if (scheduleDay == null)
            {
                return selectedDay.DayOfWeek == DayOfWeek.Friday ? AttendanceStatus.DayOff : AttendanceStatus.Absent;
            }
            return scheduleDay.IsWorkingDay ? AttendanceStatus.Absent : AttendanceStatus.DayOff;
        }
        private async Task<ReportResult<object>> EmployeesAsync(ReportRequest request)
        {
            var companyId = TenantCompanyId(ReportDefinitions.Employees, request);
            var query = _db.ApplicationUsers.AsNoTracking().Where(user => user.CompanyId.HasValue)
                .Select(user => new EmployeeReportRow
                {
                    Id = user.Id,
                    CompanyId = user.CompanyId,
                    CompanyName = user.Company != null ? user.Company.Name : string.Empty,
                    BranchId = user.BranchId,
                    BranchName = user.Branch != null ? user.Branch.Name : string.Empty,
                    EmployeeName = (user.FirstName + " " + user.LastName).Trim(),
                    Email = user.Email ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    Roles = string.Empty,
                    IsActive = user.IsActive,
                    JoinedOn = user.CreatedOn,
                    HasBoundDevice = user.EmployeeDevices.Any(device => device.IsActive)
                });
            if (companyId.HasValue) query = query.Where(x => x.CompanyId == companyId.Value);
            if (request.BranchId.HasValue) query = query.Where(x => x.BranchId == request.BranchId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active)) query = query.Where(x => x.IsActive == active);
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var roleName = request.Role.Trim();
                query = query.Where(x => _db.UserRoles.Any(userRole => userRole.UserId == x.Id && _db.Roles.Any(role => role.Id == userRole.RoleId && role.Name == roleName)));
            }
            if (request.From.HasValue) query = query.Where(x => x.JoinedOn >= request.From.Value.Date);
            if (request.To.HasValue) query = query.Where(x => x.JoinedOn <= request.To.Value.Date.AddDays(1).AddTicks(-1));
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(x => x.CompanyName.Contains(keyword) || x.BranchName.Contains(keyword) || x.EmployeeName.Contains(keyword) || x.Email.Contains(keyword) || x.PhoneNumber.Contains(keyword));
            }

            var result = await PageAsync(ReportDefinitions.Employees, SortEmployees(query, request), request);
            await HydrateRolesAsync(result.Rows.OfType<EmployeeReportRow>().ToList());
            return result;
        }

        private async Task HydrateRolesAsync(List<EmployeeReportRow> rows)
        {
            if (rows.Count == 0) return;
            var ids = rows.Select(x => x.Id).ToList();
            var roleRows = await (from userRole in _db.UserRoles.AsNoTracking()
                                  join role in _db.Roles.AsNoTracking() on userRole.RoleId equals role.Id
                                  where ids.Contains(userRole.UserId)
                                  select new { userRole.UserId, role.Name }).ToListAsync();
            foreach (var row in rows)
            {
                row.Roles = string.Join(", ", roleRows.Where(x => x.UserId == row.Id).Select(x => x.Name));
            }
        }

        private async Task<ReportResult<object>> CompaniesAsync(ReportRequest request)
        {
            var companyId = TenantCompanyId(ReportDefinitions.Companies, request);
            var now = DateTime.UtcNow;
            var query = _db.Companies.AsNoTracking().Select(company => new CompanyReportRow
            {
                Id = company.Id,
                CompanyName = company.Name,
                PlanName = company.Subscriptions.Where(s => s.IsActive).OrderByDescending(s => s.StartsOn).Select(s => s.Plan.Name).FirstOrDefault() ?? string.Empty,
                BranchesCount = company.Branches.Count,
                EmployeesCount = company.Users.Count,
                SubscriptionStatus = company.Subscriptions.Any(s => s.IsActive && (!s.EndsOn.HasValue || s.EndsOn >= now)) ? "Active" : "Inactive",
                ExpirationDate = company.Subscriptions.Where(s => s.IsActive).OrderByDescending(s => s.StartsOn).Select(s => s.EndsOn).FirstOrDefault()
            });
            if (companyId.HasValue) query = query.Where(x => x.Id == companyId.Value);
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(x => x.CompanyName.Contains(keyword) || x.PlanName.Contains(keyword));
            }
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.SubscriptionStatus == request.Status);
            return await PageAsync(ReportDefinitions.Companies, SortCompanies(query, request), request);
        }

        private async Task<ReportResult<object>> BranchesAsync(ReportRequest request)
        {
            var companyId = TenantCompanyId(ReportDefinitions.Branches, request);
            var today = DateTime.UtcNow.Date;
            var query = _db.Branches.AsNoTracking().Select(branch => new BranchReportRow
            {
                Id = branch.Id,
                CompanyId = branch.CompanyId,
                CompanyName = branch.Company.Name,
                BranchName = branch.Name,
                EmployeesCount = branch.Users.Count,
                AttendanceToday = _db.AttendanceRecords.Count(a => a.BranchId == branch.Id && a.AttendanceDate == today),
                GeoFenceStatus = branch.AllowedRadius > 0 ? "Configured" : "NotConfigured",
                IsActive = branch.IsActive
            });
            if (companyId.HasValue) query = query.Where(x => x.CompanyId == companyId.Value);
            if (request.BranchId.HasValue) query = query.Where(x => x.Id == request.BranchId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status) && bool.TryParse(request.Status, out var active)) query = query.Where(x => x.IsActive == active);
            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                var roleName = request.Role.Trim();
                query = query.Where(x => _db.UserRoles.Any(userRole => userRole.UserId == x.Id && _db.Roles.Any(role => role.Id == userRole.RoleId && role.Name == roleName)));
            }
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(x => x.CompanyName.Contains(keyword) || x.BranchName.Contains(keyword));
            }
            return await PageAsync(ReportDefinitions.Branches, SortBranches(query, request), request);
        }

        private async Task<ReportResult<object>> SubscriptionsAsync(ReportRequest request)
        {
            var companyId = TenantCompanyId(ReportDefinitions.Subscriptions, request);
            var today = DateTime.UtcNow.Date;
            var query = _db.CompanySubscriptions.AsNoTracking().Select(subscription => new SubscriptionReportRow
            {
                Id = subscription.Id,
                CompanyId = subscription.CompanyId,
                CompanyName = subscription.Company.Name,
                PlanId = subscription.PlanId,
                PlanName = subscription.Plan.Name,
                Price = subscription.Plan.Price,
                Status = subscription.IsActive && (!subscription.EndsOn.HasValue || subscription.EndsOn >= today) ? "Active" : "Inactive",
                StartDate = subscription.StartsOn,
                EndDate = subscription.EndsOn,
                RemainingDays = subscription.EndsOn.HasValue ? Math.Max(0, EF.Functions.DateDiffDay(today, subscription.EndsOn.Value.Date)) : null
            });
            if (companyId.HasValue) query = query.Where(x => x.CompanyId == companyId.Value);
            if (!string.IsNullOrWhiteSpace(request.Status)) query = query.Where(x => x.Status == request.Status);
            if (request.From.HasValue) query = query.Where(x => x.StartDate >= request.From.Value.Date);
            if (request.To.HasValue) query = query.Where(x => x.StartDate <= request.To.Value.Date.AddDays(1).AddTicks(-1));
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(x => x.CompanyName.Contains(keyword) || x.PlanName.Contains(keyword));
            }
            return await PageAsync(ReportDefinitions.Subscriptions, SortSubscriptions(query, request), request);
        }

        private async Task<ReportResult<object>> PlanUsageAsync(ReportRequest request)
        {
            var companyId = TenantCompanyId(ReportDefinitions.PlanUsage, request);
            var plans = _db.SubscriptionPlans.AsNoTracking();
            var query = plans.Select(plan => new PlanUsageReportRow
            {
                Id = plan.Id,
                PlanName = plan.Name,
                CompaniesCount = plan.CompanySubscriptions.Count(s => !companyId.HasValue || s.CompanyId == companyId.Value),
                EmployeesCount = plan.CompanySubscriptions.Where(s => !companyId.HasValue || s.CompanyId == companyId.Value).SelectMany(s => s.Company.Users).Count(),
                BranchesCount = plan.CompanySubscriptions.Where(s => !companyId.HasValue || s.CompanyId == companyId.Value).SelectMany(s => s.Company.Branches).Count(),
                AverageUsage = plan.MaxEmployees.HasValue && plan.MaxEmployees.Value > 0
                    ? Math.Round((decimal)plan.CompanySubscriptions.Where(s => !companyId.HasValue || s.CompanyId == companyId.Value).SelectMany(s => s.Company.Users).Count() / plan.MaxEmployees.Value * 100, 2)
                    : 0
            });
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                var keyword = request.Keyword.Trim();
                query = query.Where(x => x.PlanName.Contains(keyword));
            }
            return await PageAsync(ReportDefinitions.PlanUsage, SortPlanUsage(query, request), request);
        }

        private static bool TryGetSingleDay(ReportRequest request, out DateTime day)
        {
            day = (request.From ?? request.To ?? DateTime.UtcNow).Date;
            if (request.From.HasValue && request.To.HasValue) return request.From.Value.Date == request.To.Value.Date;
            return request.From.HasValue || request.To.HasValue;
        }

        private static string? NormalizeStatus(string? status) => string.IsNullOrWhiteSpace(status) ? null : status.Replace("-", string.Empty).Replace("_", string.Empty).ToLowerInvariant();

        private async Task<ReportResult<object>> PageAsync<T>(string reportKey, IQueryable<T> query, ReportRequest request)
        {
            var count = await query.CountAsync();
            var rows = await query.Skip((request.SafePage - 1) * request.SafePageSize).Take(request.SafePageSize).ToListAsync();
            return BuildResult(reportKey, rows.Cast<object>().ToList(), request, count);
        }

        private ReportResult<object> Page<T>(string reportKey, IQueryable<T> query, ReportRequest request, Func<T, object> defaultSort, bool descending)
        {
            var count = query.Count();
            query = descending ? query.OrderByDescending(defaultSort).AsQueryable() : query.OrderBy(defaultSort).AsQueryable();
            var rows = query.Skip((request.SafePage - 1) * request.SafePageSize).Take(request.SafePageSize).Cast<object>().ToList();
            return BuildResult(reportKey, rows, request, count);
        }

        private static ReportResult<object> BuildResult(string reportKey, IReadOnlyList<object> rows, ReportRequest request, int count) => new()
        {
            ReportKey = reportKey,
            Title = ReportDefinitions.Title(reportKey),
            GeneratedOn = DateTime.UtcNow,
            Rows = rows,
            Columns = ReportDefinitions.Columns(reportKey),
            AppliedFilters = AppliedFilters(request),
            Page = request.SafePage,
            PageSize = request.SafePageSize,
            TotalCount = count
        };

        private static Dictionary<string, string> AppliedFilters(ReportRequest request)
        {
            var filters = new Dictionary<string, string>();
            if (request.CompanyId.HasValue) filters["Company"] = request.CompanyId.Value.ToString();
            if (request.BranchId.HasValue) filters["Branch"] = request.BranchId.Value.ToString();
            if (request.EmployeeId.HasValue) filters["Employee"] = request.EmployeeId.Value.ToString();
            if (request.From.HasValue) filters["From"] = request.From.Value.ToString("yyyy-MM-dd");
            if (request.To.HasValue) filters["To"] = request.To.Value.ToString("yyyy-MM-dd");
            if (!string.IsNullOrWhiteSpace(request.Status)) filters["Status"] = request.Status;
            if (request.ScheduleId.HasValue) filters["Schedule"] = request.ScheduleId.Value.ToString();
            if (!string.IsNullOrWhiteSpace(request.Role)) filters["Role"] = request.Role;
            if (!string.IsNullOrWhiteSpace(request.Keyword)) filters["Search"] = request.Keyword;
            return filters;
        }

        private static IQueryable<AttendanceReportRow> SortAttendance(IQueryable<AttendanceReportRow> query, ReportRequest request) => (request.SortBy?.ToLowerInvariant(), request.IsDescending) switch
        {
            ("companyname", true) => query.OrderByDescending(x => x.CompanyName), ("companyname", false) => query.OrderBy(x => x.CompanyName),
            ("branchname", true) => query.OrderByDescending(x => x.BranchName), ("branchname", false) => query.OrderBy(x => x.BranchName),
            ("employeename", true) => query.OrderByDescending(x => x.EmployeeName), ("employeename", false) => query.OrderBy(x => x.EmployeeName),
            ("status", true) => query.OrderByDescending(x => x.Status), ("status", false) => query.OrderBy(x => x.Status),
            ("holidayname", true) => query.OrderByDescending(x => x.HolidayName), ("holidayname", false) => query.OrderBy(x => x.HolidayName),
            ("workschedulename", true) => query.OrderByDescending(x => x.WorkScheduleName), ("workschedulename", false) => query.OrderBy(x => x.WorkScheduleName),
            ("workedminutes", true) => query.OrderByDescending(x => x.WorkedMinutes), ("workedminutes", false) => query.OrderBy(x => x.WorkedMinutes),
            ("lateminutes", true) => query.OrderByDescending(x => x.LateMinutes), ("lateminutes", false) => query.OrderBy(x => x.LateMinutes),
            ("earlyleaveminutes", true) => query.OrderByDescending(x => x.EarlyLeaveMinutes), ("earlyleaveminutes", false) => query.OrderBy(x => x.EarlyLeaveMinutes),
            ("overtimeminutes", true) => query.OrderByDescending(x => x.OvertimeMinutes), ("overtimeminutes", false) => query.OrderBy(x => x.OvertimeMinutes),
            _ => query.OrderByDescending(x => x.AttendanceDate).ThenByDescending(x => x.CheckInAt)
        };

        private static IQueryable<EmployeeReportRow> SortEmployees(IQueryable<EmployeeReportRow> query, ReportRequest request) => (request.SortBy?.ToLowerInvariant(), request.IsDescending) switch
        {
            ("companyname", true) => query.OrderByDescending(x => x.CompanyName), ("companyname", false) => query.OrderBy(x => x.CompanyName),
            ("branchname", true) => query.OrderByDescending(x => x.BranchName), ("branchname", false) => query.OrderBy(x => x.BranchName),
            ("joinedon", true) => query.OrderByDescending(x => x.JoinedOn), ("joinedon", false) => query.OrderBy(x => x.JoinedOn),
            _ => request.IsDescending ? query.OrderByDescending(x => x.EmployeeName) : query.OrderBy(x => x.EmployeeName)
        };

        private static IQueryable<CompanyReportRow> SortCompanies(IQueryable<CompanyReportRow> query, ReportRequest request) => (request.SortBy?.ToLowerInvariant(), request.IsDescending) switch
        {
            ("branchescount", true) => query.OrderByDescending(x => x.BranchesCount), ("branchescount", false) => query.OrderBy(x => x.BranchesCount),
            ("employeescount", true) => query.OrderByDescending(x => x.EmployeesCount), ("employeescount", false) => query.OrderBy(x => x.EmployeesCount),
            _ => request.IsDescending ? query.OrderByDescending(x => x.CompanyName) : query.OrderBy(x => x.CompanyName)
        };

        private static IQueryable<BranchReportRow> SortBranches(IQueryable<BranchReportRow> query, ReportRequest request) => (request.SortBy?.ToLowerInvariant(), request.IsDescending) switch
        {
            ("employeescount", true) => query.OrderByDescending(x => x.EmployeesCount), ("employeescount", false) => query.OrderBy(x => x.EmployeesCount),
            ("attendancetoday", true) => query.OrderByDescending(x => x.AttendanceToday), ("attendancetoday", false) => query.OrderBy(x => x.AttendanceToday),
            _ => request.IsDescending ? query.OrderByDescending(x => x.BranchName) : query.OrderBy(x => x.BranchName)
        };

        private static IQueryable<SubscriptionReportRow> SortSubscriptions(IQueryable<SubscriptionReportRow> query, ReportRequest request) => (request.SortBy?.ToLowerInvariant(), request.IsDescending) switch
        {
            ("price", true) => query.OrderByDescending(x => x.Price), ("price", false) => query.OrderBy(x => x.Price),
            ("remainingdays", true) => query.OrderByDescending(x => x.RemainingDays), ("remainingdays", false) => query.OrderBy(x => x.RemainingDays),
            ("enddate", true) => query.OrderByDescending(x => x.EndDate), ("enddate", false) => query.OrderBy(x => x.EndDate),
            _ => request.IsDescending ? query.OrderByDescending(x => x.StartDate) : query.OrderBy(x => x.StartDate)
        };

        private static IQueryable<PlanUsageReportRow> SortPlanUsage(IQueryable<PlanUsageReportRow> query, ReportRequest request) => (request.SortBy?.ToLowerInvariant(), request.IsDescending) switch
        {
            ("companiescount", true) => query.OrderByDescending(x => x.CompaniesCount), ("companiescount", false) => query.OrderBy(x => x.CompaniesCount),
            ("employeescount", true) => query.OrderByDescending(x => x.EmployeesCount), ("employeescount", false) => query.OrderBy(x => x.EmployeesCount),
            ("averageusage", true) => query.OrderByDescending(x => x.AverageUsage), ("averageusage", false) => query.OrderBy(x => x.AverageUsage),
            _ => request.IsDescending ? query.OrderByDescending(x => x.PlanName) : query.OrderBy(x => x.PlanName)
        };
    }
}



