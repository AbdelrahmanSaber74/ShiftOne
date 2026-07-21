using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Branches;
using ShiftOne.Core.Entities.Companies;
using ShiftOne.Core.Entities.WorkSchedules;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.WorkSchedules;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.WorkSchedules;

namespace ShiftOne.Application.Services.WorkSchedules
{
    public class WorkScheduleService : IWorkScheduleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public WorkScheduleService(IUnitOfWork unitOfWork, ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword, Guid? companyId, Guid? branchId, bool? isActive)
        {
            var targetCompanyId = companyId ?? (_tenantContext.IsPlatformAdmin ? null : _tenantContext.CompanyId);
            if (!targetCompanyId.HasValue && !_tenantContext.IsPlatformAdmin)
            {
                return GeneralResponse.BadRequest("Messages.CompanyContextRequired");
            }

            var spec = targetCompanyId.HasValue
                ? Spec.ForChain<WorkSchedule>(schedule => schedule.CompanyId == targetCompanyId.Value,
                    query => query.Include(schedule => schedule.Company),
                    query => query.Include(schedule => schedule.Days))
                : Spec.ForChain<WorkSchedule>(schedule => true,
                    query => query.Include(schedule => schedule.Company),
                    query => query.Include(schedule => schedule.Days));

            if (isActive.HasValue) spec.AddCriteria(schedule => schedule.IsActive == isActive.Value);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var search = keyword.Trim();
                spec.AddCriteria(schedule => schedule.Name.Contains(search) || (schedule.Description != null && schedule.Description.Contains(search)));
            }

            var countSpec = Spec.For<WorkSchedule>(spec.Criteria);
            spec.ApplyOrderByDescending(schedule => schedule.IsDefault);
            spec.ApplyPaging((request.Page - 1) * request.PageSize, request.PageSize);

            var rows = (await _unitOfWork.Repository<WorkSchedule>().GetAllAsync(spec)).Select(Map).ToList();
            var count = await _unitOfWork.Repository<WorkSchedule>().CountAsync(countSpec);
            return GeneralResponse.Ok("Messages.Success", rows, request.Page, request.PageSize, count);
        }

        public async Task<GeneralResponse> GetByIdAsync(Guid id)
        {
            var schedule = await GetScheduleAsync(id);
            return schedule == null ? GeneralResponse.NotFound("Messages.NotFound") : GeneralResponse.Ok("Messages.Success", Map(schedule));
        }

        public async Task<GeneralResponse> CreateAsync(UpsertWorkScheduleRequest request)
        {
            var validation = await ValidateRequestAsync(request, null);
            if (!validation.Success) return validation.Response!;

            var schedule = new WorkSchedule
            {
                Id = Guid.NewGuid(),
                CompanyId = validation.CompanyId,
                Name = request.Name.Trim(),
                Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
                TimeZoneId = NormalizeTimeZone(request.TimeZoneId),
                IsDefault = request.IsDefault,
                IsActive = request.IsActive,
                Days = request.Days.Select(MapDay).ToList()
            };

            if (schedule.IsDefault && schedule.IsActive) await ClearDefaultAsync(schedule.CompanyId, null);
            await _unitOfWork.Repository<WorkSchedule>().AddAsync(schedule);
            await _unitOfWork.CompleteAsync();
            if (request.BranchId.HasValue) await AssignToBranchAsync(request.BranchId.Value, new AssignBranchScheduleRequest { WorkScheduleId = schedule.Id });
            return GeneralResponse.Ok("Messages.Success", Map(schedule));
        }

        public async Task<GeneralResponse> UpdateAsync(Guid id, UpsertWorkScheduleRequest request)
        {
            var schedule = await GetScheduleAsync(id);
            if (schedule == null) return GeneralResponse.NotFound("Messages.NotFound");

            var validation = await ValidateRequestAsync(request, schedule.CompanyId);
            if (!validation.Success) return validation.Response!;

            schedule.Name = request.Name.Trim();
            schedule.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
            schedule.TimeZoneId = NormalizeTimeZone(request.TimeZoneId);
            schedule.IsDefault = request.IsDefault;
            schedule.IsActive = request.IsActive;

            var requestDaysMap = request.Days.ToDictionary(d => d.DayOfWeek);
            foreach (var existingDay in schedule.Days)
            {
                if (requestDaysMap.TryGetValue(existingDay.DayOfWeek, out var reqDay))
                {
                    existingDay.IsWorkingDay = reqDay.IsWorkingDay;
                    existingDay.StartTime = reqDay.IsWorkingDay ? reqDay.StartTime : null;
                    existingDay.EndTime = reqDay.IsWorkingDay ? reqDay.EndTime : null;
                    existingDay.LateGraceMinutes = Math.Max(0, reqDay.LateGraceMinutes);
                    existingDay.EarlyLeaveGraceMinutes = Math.Max(0, reqDay.EarlyLeaveGraceMinutes);
                    existingDay.MinimumWorkingMinutes = reqDay.IsWorkingDay ? Math.Max(0, reqDay.MinimumWorkingMinutes) : 0;
                    existingDay.OvertimeEnabled = reqDay.IsWorkingDay && reqDay.OvertimeEnabled;
                    requestDaysMap.Remove(existingDay.DayOfWeek);
                }
            }

            foreach (var remainingReq in requestDaysMap.Values)
            {
                schedule.Days.Add(MapDay(remainingReq));
            }

            if (schedule.IsDefault && schedule.IsActive) await ClearDefaultAsync(schedule.CompanyId, schedule.Id);
            await _unitOfWork.Repository<WorkSchedule>().UpdateAsync(schedule);
            await _unitOfWork.CompleteAsync();
            if (request.BranchId.HasValue) await AssignToBranchAsync(request.BranchId.Value, new AssignBranchScheduleRequest { WorkScheduleId = schedule.Id });
            return GeneralResponse.Ok("Messages.Success", Map(schedule));
        }

        public async Task<GeneralResponse> DeleteAsync(Guid id)
        {
            var schedule = await GetScheduleAsync(id);
            if (schedule == null) return GeneralResponse.NotFound("Messages.NotFound");
            schedule.IsActive = false;
            schedule.IsDefault = false;
            await _unitOfWork.Repository<WorkSchedule>().UpdateAsync(schedule);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success");
        }

        public async Task<GeneralResponse> SetDefaultAsync(Guid id)
        {
            var schedule = await GetScheduleAsync(id);
            if (schedule == null) return GeneralResponse.NotFound("Messages.NotFound");
            if (!schedule.IsActive) return GeneralResponse.BadRequest("Messages.WorkScheduleInactive");
            await ClearDefaultAsync(schedule.CompanyId, schedule.Id);
            schedule.IsDefault = true;
            await _unitOfWork.Repository<WorkSchedule>().UpdateAsync(schedule);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success", Map(schedule));
        }

        public async Task<GeneralResponse> AssignToBranchAsync(Guid branchId, AssignBranchScheduleRequest request)
        {
            var branch = await GetBranchAsync(branchId);
            if (branch == null) return GeneralResponse.NotFound("Messages.NotFound");
            if (request.WorkScheduleId.HasValue)
            {
                var schedule = await GetScheduleAsync(request.WorkScheduleId.Value);
                if (schedule == null || schedule.CompanyId != branch.CompanyId || !schedule.IsActive) return GeneralResponse.BadRequest("Messages.InvalidWorkSchedule");
                branch.WorkScheduleId = schedule.Id;
            }
            else branch.WorkScheduleId = null;

            await _unitOfWork.Repository<Branch>().UpdateAsync(branch);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success");
        }

        public Task<GeneralResponse> ClearBranchScheduleAsync(Guid branchId) => AssignToBranchAsync(branchId, new AssignBranchScheduleRequest());

        private Guid? ResolveCompanyId(Guid? requestedCompanyId) => _tenantContext.IsPlatformAdmin ? requestedCompanyId : _tenantContext.CompanyId;

        private async Task<WorkSchedule?> GetScheduleAsync(Guid id)
        {
            var spec = Spec.ForChain<WorkSchedule>(schedule => schedule.Id == id,
                query => query.Include(schedule => schedule.Company),
                query => query.Include(schedule => schedule.Days));
            var schedule = (await _unitOfWork.Repository<WorkSchedule>().GetAllAsync(spec)).SingleOrDefault();
            return schedule != null && _tenantContext.CanAccessCompany(schedule.CompanyId) ? schedule : null;
        }

        private async Task<Branch?> GetBranchAsync(Guid branchId)
        {
            var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(branchId);
            return branch != null && _tenantContext.CanAccessCompany(branch.CompanyId) ? branch : null;
        }

        private async Task<ValidationResult> ValidateRequestAsync(UpsertWorkScheduleRequest request, Guid? existingCompanyId)
        {
            var companyId = existingCompanyId ?? ResolveCompanyId(request.CompanyId);
            if (!companyId.HasValue) return ValidationResult.Fail(GeneralResponse.BadRequest("Messages.CompanyContextRequired"));
            if (!_tenantContext.CanAccessCompany(companyId)) return ValidationResult.Fail(GeneralResponse.Unauthorized("Messages.CompanyContextRequired"));
            if (string.IsNullOrWhiteSpace(request.Name)) return ValidationResult.Fail(GeneralResponse.BadRequest("Messages.NameRequired"));
            if (request.Days.Count == 0 || !request.Days.Any(day => day.IsWorkingDay)) return ValidationResult.Fail(GeneralResponse.BadRequest("Messages.WorkScheduleWorkingDayRequired"));
            if (await _unitOfWork.Repository<Company>().GetByIdAsync(companyId.Value) == null) return ValidationResult.Fail(GeneralResponse.BadRequest("Messages.CompanyContextRequired"));
            if (request.BranchId.HasValue)
            {
                var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(request.BranchId.Value);
                if (branch == null || branch.CompanyId != companyId.Value) return ValidationResult.Fail(GeneralResponse.BadRequest("Messages.InvalidBranch"));
            }
            foreach (var day in request.Days.Where(day => day.IsWorkingDay))
            {
                if (!day.StartTime.HasValue || !day.EndTime.HasValue || day.StartTime >= day.EndTime) return ValidationResult.Fail(GeneralResponse.BadRequest("Messages.WorkScheduleInvalidTimeRange"));
                if (day.LateGraceMinutes < 0 || day.EarlyLeaveGraceMinutes < 0 || day.MinimumWorkingMinutes < 0) return ValidationResult.Fail(GeneralResponse.BadRequest("Messages.WorkScheduleInvalidRules"));
            }
            return ValidationResult.Ok(companyId.Value);
        }

        private async Task ClearDefaultAsync(Guid companyId, Guid? exceptId)
        {
            var defaults = await _unitOfWork.Repository<WorkSchedule>().GetAllAsync(Spec.For<WorkSchedule>(schedule => schedule.CompanyId == companyId && schedule.IsDefault && schedule.IsActive && (!exceptId.HasValue || schedule.Id != exceptId.Value)));
            foreach (var item in defaults)
            {
                item.IsDefault = false;
                await _unitOfWork.Repository<WorkSchedule>().UpdateAsync(item);
            }
        }

        private static string NormalizeTimeZone(string? timeZoneId) => string.IsNullOrWhiteSpace(timeZoneId) ? "Arab Standard Time" : timeZoneId.Trim();
        private static WorkScheduleDay MapDay(WorkScheduleDayRequest request) => new()
        {
            Id = Guid.NewGuid(),
            DayOfWeek = request.DayOfWeek,
            IsWorkingDay = request.IsWorkingDay,
            StartTime = request.IsWorkingDay ? request.StartTime : null,
            EndTime = request.IsWorkingDay ? request.EndTime : null,
            LateGraceMinutes = Math.Max(0, request.LateGraceMinutes),
            EarlyLeaveGraceMinutes = Math.Max(0, request.EarlyLeaveGraceMinutes),
            MinimumWorkingMinutes = request.IsWorkingDay ? Math.Max(0, request.MinimumWorkingMinutes) : 0,
            OvertimeEnabled = request.IsWorkingDay && request.OvertimeEnabled
        };

        private static WorkScheduleResponse Map(WorkSchedule schedule) => new()
        {
            Id = schedule.Id,
            CompanyId = schedule.CompanyId,
            CompanyName = schedule.Company?.Name ?? string.Empty,
            BranchId = null,
            BranchName = null,
            Name = schedule.Name,
            Description = schedule.Description,
            TimeZoneId = schedule.TimeZoneId,
            IsDefault = schedule.IsDefault,
            IsActive = schedule.IsActive,
            CreatedOn = schedule.CreatedOn,
            WorkingDaysCount = schedule.Days.Count(day => day.IsWorkingDay),
            Days = schedule.Days.OrderBy(day => day.DayOfWeek).Select(day => new WorkScheduleDayResponse
            {
                Id = day.Id,
                DayOfWeek = day.DayOfWeek,
                IsWorkingDay = day.IsWorkingDay,
                StartTime = day.StartTime,
                EndTime = day.EndTime,
                LateGraceMinutes = day.LateGraceMinutes,
                EarlyLeaveGraceMinutes = day.EarlyLeaveGraceMinutes,
                MinimumWorkingMinutes = day.MinimumWorkingMinutes,
                OvertimeEnabled = day.OvertimeEnabled
            }).ToList()
        };

        private sealed class ValidationResult
        {
            public bool Success { get; private set; }
            public Guid CompanyId { get; private set; }
            public GeneralResponse? Response { get; private set; }
            public static ValidationResult Ok(Guid companyId) => new() { Success = true, CompanyId = companyId };
            public static ValidationResult Fail(GeneralResponse response) => new() { Success = false, Response = response };
        }
    }
}
