using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Branches;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Employees;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.Employees;

namespace ShiftOne.Application.Services.Saas
{
    public class EmployeeManagementService : IEmployeeManagementService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;
        private readonly IPlanLimitService _planLimitService;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeeManagementService(IUnitOfWork unitOfWork, ITenantContext tenantContext, IPlanLimitService planLimitService, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
            _planLimitService = planLimitService;
            _userManager = userManager;
        }

        public async Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword, bool? isActive, Guid? branchId)
        {
            var companyId = _tenantContext.ResolveCompanyId(null);
            IQueryable<ApplicationUser> query;

            if (companyId.HasValue)
            {
                query = _userManager.Users.Include(user => user.Branch).Where(user => user.CompanyId == companyId.Value);
            }
            else if (_tenantContext.IsPlatformAdmin)
            {
                query = _userManager.Users.Include(user => user.Branch);
            }
            else
            {
                return GeneralResponse.BadRequest("Messages.CompanyContextRequired");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var clean = keyword.Trim();
                query = query.Where(user => user.FirstName.Contains(clean) || user.LastName.Contains(clean) || (user.Email != null && user.Email.Contains(clean)) || (user.PhoneNumber != null && user.PhoneNumber.Contains(clean)));
            }
            if (isActive.HasValue)
            {
                query = query.Where(user => user.IsActive == isActive.Value);
            }
            if (branchId.HasValue)
            {
                query = query.Where(user => user.BranchId == branchId.Value);
            }

            var count = await query.CountAsync();
            var users = await query.OrderBy(user => user.FirstName)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();
            var data = new List<EmployeeResponse>();
            foreach (var user in users)
            {
                data.Add(await MapAsync(user));
            }

            return GeneralResponse.Ok("Messages.Success", data, request.Page, request.PageSize, count);
        }

        public async Task<GeneralResponse> GetByIdAsync(Guid id)
        {
            var user = await GetTenantUserAsync(id);
            return user == null ? GeneralResponse.NotFound("Messages.UserNotFound") : GeneralResponse.Ok("Messages.Success", await MapAsync(user));
        }

        public async Task<GeneralResponse> CreateAsync(CreateEmployeeRequest request)
        {
            var companyId = _tenantContext.ResolveCompanyId(null);
            if (!companyId.HasValue)
            {
                return GeneralResponse.BadRequest("Messages.CompanyContextRequired");
            }

            var role = NormalizeRole(request.Role);
            if (role == null)
            {
                return GeneralResponse.BadRequest("Messages.InvalidRequest");
            }

            if (_tenantContext.IsHr && role != Roles.Employee.ToString())
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            if (role == Roles.Employee.ToString() && !await _planLimitService.CanCreateEmployeeAsync(companyId.Value))
            {
                return GeneralResponse.BadRequest("Messages.PlanLimitExceeded");
            }
            if (role == Roles.HR.ToString() && !await _planLimitService.CanCreateHrAsync(companyId.Value))
            {
                return GeneralResponse.BadRequest("Messages.PlanLimitExceeded");
            }
            if (role == Roles.CompanyAdmin.ToString() && !await _planLimitService.CanCreateCompanyAdminAsync(companyId.Value))
            {
                return GeneralResponse.BadRequest("Messages.PlanLimitExceeded");
            }

            if (role == Roles.Employee.ToString() && !request.BranchId.HasValue)
            {
                return GeneralResponse.BadRequest("Messages.BranchRequiredForEmployee");
            }

            var branch = request.BranchId.HasValue
                ? await GetTenantBranchAsync(request.BranchId.Value, companyId.Value)
                : null;
            if (request.BranchId.HasValue && branch == null)
            {
                return GeneralResponse.BadRequest("Messages.InvalidBranch");
            }

            var userName = !string.IsNullOrWhiteSpace(request.Email) ? request.Email.Trim() : Guid.NewGuid().ToString();
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = request.Email?.Trim(),
                EmailConfirmed = !string.IsNullOrWhiteSpace(request.Email),
                PhoneNumber = request.PhoneNumber?.Trim(),
                PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(request.PhoneNumber),
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                CompanyId = companyId.Value,
                BranchId = branch?.Id,
                IsActive = request.IsActive
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return GeneralResponse.BadRequest("Messages.InvalidRequest", result.Errors);
            }

            await _userManager.AddToRoleAsync(user, role);
            return GeneralResponse.Ok("Messages.Success", await MapAsync(user));
        }

        public async Task<GeneralResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request)
        {
            var user = await GetTenantUserAsync(id);
            if (user == null || !user.CompanyId.HasValue)
            {
                return GeneralResponse.NotFound("Messages.UserNotFound");
            }

            var branch = request.BranchId.HasValue
                ? await GetTenantBranchAsync(request.BranchId.Value, user.CompanyId.Value)
                : null;
            if (request.BranchId.HasValue && branch == null)
            {
                return GeneralResponse.BadRequest("Messages.InvalidBranch");
            }

            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Email = request.Email?.Trim();
            user.UserName = !string.IsNullOrWhiteSpace(user.Email) ? user.Email : user.UserName;
            user.PhoneNumber = request.PhoneNumber?.Trim();
            user.BranchId = branch?.Id;
            user.IsActive = request.IsActive;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return GeneralResponse.BadRequest("Messages.InvalidRequest", result.Errors);
            }

            return GeneralResponse.Ok("Messages.Success", await MapAsync(user));
        }

        public async Task<GeneralResponse> DeleteAsync(Guid id)
        {
            var user = await GetTenantUserAsync(id);
            if (user == null)
            {
                return GeneralResponse.NotFound("Messages.UserNotFound");
            }

            user.IsActive = false;
            await _userManager.UpdateAsync(user);
            return GeneralResponse.Ok("Messages.Success");
        }

        public async Task<GeneralResponse> ResetDeviceAsync(Guid employeeId)
        {
            var user = await GetTenantUserAsync(employeeId);
            if (user == null)
            {
                return GeneralResponse.NotFound("Messages.UserNotFound");
            }

            var devices = await _unitOfWork.Repository<EmployeeDevice>().GetAllAsync(Spec.For<EmployeeDevice>(device => device.EmployeeId == employeeId && device.IsActive));
            foreach (var device in devices)
            {
                device.IsActive = false;
                device.ResetOn = DateTime.UtcNow;
                device.ResetBy = _tenantContext.UserId;
                await _unitOfWork.Repository<EmployeeDevice>().UpdateAsync(device);
            }

            await _unitOfWork.CompleteAsync();
            await _userManager.UpdateSecurityStampAsync(user);
            return GeneralResponse.Ok("Messages.Success");
        }

        private static string? NormalizeRole(string role)
        {
            if (string.Equals(role, Roles.Employee.ToString(), StringComparison.OrdinalIgnoreCase)) return Roles.Employee.ToString();
            if (string.Equals(role, Roles.HR.ToString(), StringComparison.OrdinalIgnoreCase)) return Roles.HR.ToString();
            if (string.Equals(role, Roles.CompanyAdmin.ToString(), StringComparison.OrdinalIgnoreCase)) return Roles.CompanyAdmin.ToString();
            return null;
        }

        private async Task<ApplicationUser?> GetTenantUserAsync(Guid id)
        {
            var companyId = _tenantContext.ResolveCompanyId(null);
            if (!companyId.HasValue)
            {
                return null;
            }

            return await _userManager.Users.Include(user => user.Branch).SingleOrDefaultAsync(user => user.Id == id && user.CompanyId == companyId.Value);
        }

        private async Task<Branch?> GetTenantBranchAsync(Guid branchId, Guid companyId)
        {
            return (await _unitOfWork.Repository<Branch>().GetAllAsync(Spec.For<Branch>(branch => branch.Id == branchId && branch.CompanyId == companyId && branch.IsActive))).SingleOrDefault();
        }

        private async Task<EmployeeResponse> MapAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var hasDevice = await _unitOfWork.Repository<EmployeeDevice>().CountAsync(Spec.For<EmployeeDevice>(device => device.EmployeeId == user.Id && device.IsActive)) > 0;
            return new EmployeeResponse
            {
                Id = user.Id,
                CompanyId = user.CompanyId,
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                IsActive = user.IsActive,
                Roles = roles.ToList(),
                HasBoundDevice = hasDevice
            };
        }
    }
}