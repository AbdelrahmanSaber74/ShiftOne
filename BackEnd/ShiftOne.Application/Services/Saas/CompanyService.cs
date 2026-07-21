using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Companies;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Entities.Subscriptions;
using ShiftOne.Core.Entities.WorkSchedules;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Companies;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.Companies;

namespace ShiftOne.Application.Services.Saas
{
    public class CompanyService : ICompanyService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPlanLimitService _planLimitService;

        public CompanyService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager, IPlanLimitService planLimitService)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _planLimitService = planLimitService;
        }

        public async Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword, bool? isActive)
        {
            var spec = Spec.ForChain<Company>(null, query => query.Include(company => company.Subscriptions).ThenInclude(subscription => subscription.Plan));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var clean = keyword.Trim();
                spec.AddCriteria(company => company.Name.Contains(clean) || company.Code.Contains(clean));
            }
            if (isActive.HasValue)
            {
                spec.AddCriteria(company => company.IsActive == isActive.Value);
            }

            var countSpec = Spec.For<Company>(spec.Criteria);
            spec.ApplyOrderBy(company => company.Name);
            spec.ApplyPaging((request.Page - 1) * request.PageSize, request.PageSize);
            var companies = (await _unitOfWork.Repository<Company>().GetAllAsync(spec)).Select(Map).ToList();
            var count = await _unitOfWork.Repository<Company>().CountAsync(countSpec);
            return GeneralResponse.Ok("Messages.Success", companies, request.Page, request.PageSize, count);
        }

        public async Task<GeneralResponse> GetByIdAsync(Guid id)
        {
            var spec = Spec.ForChain<Company>(company => company.Id == id, query => query.Include(company => company.Subscriptions).ThenInclude(subscription => subscription.Plan));
            var company = (await _unitOfWork.Repository<Company>().GetAllAsync(spec)).SingleOrDefault();
            return company == null ? GeneralResponse.NotFound("Messages.NotFound") : GeneralResponse.Ok("Messages.Success", Map(company));
        }

        public async Task<GeneralResponse> CreateAsync(CreateCompanyRequest request)
        {
            var exists = await _unitOfWork.Repository<Company>().CountAsync(Spec.For<Company>(company => company.Code == request.Code.Trim())) > 0;
            if (exists)
            {
                return GeneralResponse.BadRequest("Messages.CompanyCodeInUse");
            }

            var company = new Company
            {
                Name = request.Name.Trim(),
                Code = request.Code.Trim(),
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                IsActive = request.IsActive
            };

            await _unitOfWork.Repository<Company>().AddAsync(company);
            await _unitOfWork.CompleteAsync();

            var defaultSchedule = new WorkSchedule
            {
                CompanyId = company.Id,
                Name = "الشيفت الصباحي",
                Description = "جدول مواعيد العمل الصباحي الافتراضي للشركة",
                TimeZoneId = "Arab Standard Time",
                IsDefault = true,
                IsActive = true,
                Days = new List<WorkScheduleDay>
                {
                    new WorkScheduleDay { DayOfWeek = DayOfWeek.Sunday, IsWorkingDay = true, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), LateGraceMinutes = 15, EarlyLeaveGraceMinutes = 15, MinimumWorkingMinutes = 480, OvertimeEnabled = true },
                    new WorkScheduleDay { DayOfWeek = DayOfWeek.Monday, IsWorkingDay = true, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), LateGraceMinutes = 15, EarlyLeaveGraceMinutes = 15, MinimumWorkingMinutes = 480, OvertimeEnabled = true },
                    new WorkScheduleDay { DayOfWeek = DayOfWeek.Tuesday, IsWorkingDay = true, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), LateGraceMinutes = 15, EarlyLeaveGraceMinutes = 15, MinimumWorkingMinutes = 480, OvertimeEnabled = true },
                    new WorkScheduleDay { DayOfWeek = DayOfWeek.Wednesday, IsWorkingDay = true, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), LateGraceMinutes = 15, EarlyLeaveGraceMinutes = 15, MinimumWorkingMinutes = 480, OvertimeEnabled = true },
                    new WorkScheduleDay { DayOfWeek = DayOfWeek.Thursday, IsWorkingDay = true, StartTime = new TimeSpan(9, 0, 0), EndTime = new TimeSpan(17, 0, 0), LateGraceMinutes = 15, EarlyLeaveGraceMinutes = 15, MinimumWorkingMinutes = 480, OvertimeEnabled = true },
                    new WorkScheduleDay { DayOfWeek = DayOfWeek.Friday, IsWorkingDay = false },
                    new WorkScheduleDay { DayOfWeek = DayOfWeek.Saturday, IsWorkingDay = false }
                }
            };

            await _unitOfWork.Repository<WorkSchedule>().AddAsync(defaultSchedule);
            await _unitOfWork.CompleteAsync();

            if (request.PlanId.HasValue)
            {
                await _unitOfWork.Repository<CompanySubscription>().AddAsync(new CompanySubscription
                {
                    CompanyId = company.Id,
                    PlanId = request.PlanId.Value,
                    StartsOn = DateTime.UtcNow,
                    IsActive = true
                });
                await _unitOfWork.CompleteAsync();
            }

            if (!string.IsNullOrWhiteSpace(request.AdminEmail) && !string.IsNullOrWhiteSpace(request.AdminPassword))
            {
                if (!await _planLimitService.CanCreateCompanyAdminAsync(company.Id))
                {
                    return GeneralResponse.BadRequest("Messages.PlanLimitExceeded");
                }

                var admin = new ApplicationUser
                {
                    UserName = request.AdminEmail.Trim(),
                    Email = request.AdminEmail.Trim(),
                    EmailConfirmed = true,
                    FirstName = string.IsNullOrWhiteSpace(request.AdminFirstName) ? "Company" : request.AdminFirstName.Trim(),
                    LastName = string.IsNullOrWhiteSpace(request.AdminLastName) ? "Admin" : request.AdminLastName.Trim(),
                    IsActive = true,
                    CompanyId = company.Id
                };
                var result = await _userManager.CreateAsync(admin, request.AdminPassword);
                if (!result.Succeeded)
                {
                    return GeneralResponse.BadRequest("Messages.InvalidRequest", result.Errors);
                }
                await _userManager.AddToRoleAsync(admin, Roles.CompanyAdmin.ToString());
            }

            return GeneralResponse.Ok("Messages.Success", Map(company));
        }

        public async Task<GeneralResponse> UpdateAsync(Guid id, UpdateCompanyRequest request)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(id);
            if (company == null)
            {
                return GeneralResponse.NotFound("Messages.NotFound");
            }

            company.Name = request.Name.Trim();
            company.Code = request.Code.Trim();
            company.Email = request.Email;
            company.PhoneNumber = request.PhoneNumber;
            company.Address = request.Address;
            company.IsActive = request.IsActive;
            await _unitOfWork.Repository<Company>().UpdateAsync(company);
            await _unitOfWork.CompleteAsync();

            if (request.PlanId.HasValue)
            {
                var activeSubscriptions = await _unitOfWork.Repository<CompanySubscription>().GetAllAsync(
                    Spec.For<CompanySubscription>(sub => sub.CompanyId == id && sub.IsActive)
                );
                var currentSubscription = activeSubscriptions
                    .Where(sub => sub.EndsOn == null || sub.EndsOn >= DateTime.UtcNow)
                    .OrderByDescending(sub => sub.StartsOn)
                    .FirstOrDefault();

                if (currentSubscription == null || currentSubscription.PlanId != request.PlanId.Value)
                {
                    foreach (var sub in activeSubscriptions)
                    {
                        sub.IsActive = false;
                        sub.EndsOn = DateTime.UtcNow;
                        await _unitOfWork.Repository<CompanySubscription>().UpdateAsync(sub);
                    }

                    await _unitOfWork.Repository<CompanySubscription>().AddAsync(new CompanySubscription
                    {
                        CompanyId = id,
                        PlanId = request.PlanId.Value,
                        StartsOn = DateTime.UtcNow,
                        IsActive = true
                    });
                    
                    await _unitOfWork.CompleteAsync();
                }
            }

            var spec = Spec.ForChain<Company>(c => c.Id == id, query => query.Include(c => c.Subscriptions).ThenInclude(sub => sub.Plan));
            var updatedCompany = (await _unitOfWork.Repository<Company>().GetAllAsync(spec)).SingleOrDefault();

            return GeneralResponse.Ok("Messages.Success", Map(updatedCompany ?? company));
        }

        public async Task<GeneralResponse> DeleteAsync(Guid id)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(id);
            if (company == null)
            {
                return GeneralResponse.NotFound("Messages.NotFound");
            }

            company.IsActive = false;
            await _unitOfWork.Repository<Company>().UpdateAsync(company);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success");
        }

        private static CompanyResponse Map(Company company)
        {
            var plan = company.Subscriptions
                .Where(subscription => subscription.IsActive && (subscription.EndsOn == null || subscription.EndsOn >= DateTime.UtcNow))
                .OrderByDescending(subscription => subscription.StartsOn)
                .FirstOrDefault()?.Plan;

            return new CompanyResponse
            {
                Id = company.Id,
                Name = company.Name,
                Code = company.Code,
                Email = company.Email,
                PhoneNumber = company.PhoneNumber,
                Address = company.Address,
                IsActive = company.IsActive,
                CurrentPlanName = plan?.Name,
                CurrentPlanId = plan?.Id
            };
        }
    }
}
