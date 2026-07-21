using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Subscriptions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Subscriptions;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.Subscriptions;

namespace ShiftOne.Application.Services.Saas
{
    public class CompanySubscriptionService : ICompanySubscriptionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;

        public CompanySubscriptionService(IUnitOfWork unitOfWork, ITenantContext tenantContext)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
        }

        public async Task<GeneralResponse> GetAllAsync(PaginationRequest request, Guid? companyId)
        {
            Guid? resolvedCompanyId;
            try
            {
                resolvedCompanyId = _tenantContext.ResolveCompanyId(companyId);
            }
            catch (UnauthorizedAccessException)
            {
                return GeneralResponse.Unauthorized("Messages.CompanyContextRequired");
            }

            var spec = Spec.ForChain<CompanySubscription>(
                resolvedCompanyId.HasValue ? subscription => subscription.CompanyId == resolvedCompanyId.Value : null,
                query => query.Include(subscription => subscription.Company),
                query => query.Include(subscription => subscription.Plan));
            var countSpec = Spec.For<CompanySubscription>(spec.Criteria);
            spec.ApplyOrderByDescending(subscription => subscription.StartsOn);
            spec.ApplyPaging((request.Page - 1) * request.PageSize, request.PageSize);
            var data = (await _unitOfWork.Repository<CompanySubscription>().GetAllAsync(spec)).Select(Map).ToList();
            var count = await _unitOfWork.Repository<CompanySubscription>().CountAsync(countSpec);
            return GeneralResponse.Ok("Messages.Success", data, request.Page, request.PageSize, count);
        }

        public async Task<GeneralResponse> AssignAsync(AssignCompanySubscriptionRequest request)
        {
            if (!_tenantContext.IsPlatformAdmin)
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            var existingSpec = Spec.For<CompanySubscription>(subscription => subscription.CompanyId == request.CompanyId && subscription.IsActive);
            var existing = await _unitOfWork.Repository<CompanySubscription>().GetAllAsync(existingSpec);
            foreach (var subscription in existing)
            {
                subscription.IsActive = false;
                subscription.EndsOn ??= DateTime.UtcNow;
                await _unitOfWork.Repository<CompanySubscription>().UpdateAsync(subscription);
            }

            var newSubscription = new CompanySubscription
            {
                CompanyId = request.CompanyId,
                PlanId = request.PlanId,
                StartsOn = request.StartsOn,
                EndsOn = request.EndsOn,
                IsActive = request.IsActive
            };
            await _unitOfWork.Repository<CompanySubscription>().AddAsync(newSubscription);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success", new { newSubscription.Id });
        }

        private static CompanySubscriptionResponse Map(CompanySubscription subscription) => new()
        {
            Id = subscription.Id,
            CompanyId = subscription.CompanyId,
            CompanyName = subscription.Company.Name,
            PlanId = subscription.PlanId,
            PlanName = subscription.Plan.Name,
            StartsOn = subscription.StartsOn,
            EndsOn = subscription.EndsOn,
            IsActive = subscription.IsActive
        };
    }
}