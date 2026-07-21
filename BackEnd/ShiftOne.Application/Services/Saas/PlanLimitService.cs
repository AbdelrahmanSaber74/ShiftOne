using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Branches;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Entities.Subscriptions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Constants;

namespace ShiftOne.Application.Services.Saas
{
    public class PlanLimitService : IPlanLimitService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;

        public PlanLimitService(IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task<bool> CanCreateBranchAsync(Guid companyId)
        {
            var plan = await GetActivePlanAsync(companyId);
            if (plan?.MaxBranches == null)
            {
                return true;
            }

            var count = await _unitOfWork.Repository<Branch>().CountAsync(Spec.For<Branch>(branch => branch.CompanyId == companyId));
            return count < plan.MaxBranches.Value;
        }

        public async Task<bool> CanCreateEmployeeAsync(Guid companyId)
        {
            var plan = await GetActivePlanAsync(companyId);
            if (plan?.MaxEmployees == null)
            {
                return true;
            }

            var users = await _userManager.GetUsersInRoleAsync(Roles.Employee.ToString());
            return users.Count(user => user.CompanyId == companyId) < plan.MaxEmployees.Value;
        }

        public async Task<bool> CanCreateHrAsync(Guid companyId)
        {
            var plan = await GetActivePlanAsync(companyId);
            if (plan?.MaxHRUsers == null)
            {
                return true;
            }

            var users = await _userManager.GetUsersInRoleAsync(Roles.HR.ToString());
            return users.Count(user => user.CompanyId == companyId) < plan.MaxHRUsers.Value;
        }

        public async Task<bool> CanCreateCompanyAdminAsync(Guid companyId)
        {
            var plan = await GetActivePlanAsync(companyId);
            if (plan?.MaxCompanyAdmins == null)
            {
                return true;
            }

            var users = await _userManager.GetUsersInRoleAsync(Roles.CompanyAdmin.ToString());
            return users.Count(user => user.CompanyId == companyId) < plan.MaxCompanyAdmins.Value;
        }

        private async Task<SubscriptionPlan?> GetActivePlanAsync(Guid companyId)
        {
            var spec = Spec.ForChain<CompanySubscription>(
                subscription => subscription.CompanyId == companyId &&
                                subscription.IsActive &&
                                (subscription.EndsOn == null || subscription.EndsOn >= DateTime.UtcNow),
                query => query.Include(subscription => subscription.Plan));
            var subscription = (await _unitOfWork.Repository<CompanySubscription>().GetAllAsync(spec))
                .OrderByDescending(x => x.StartsOn)
                .FirstOrDefault();

            return subscription?.Plan;
        }
    }
}
