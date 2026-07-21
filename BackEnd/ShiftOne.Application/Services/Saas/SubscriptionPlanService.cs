using ShiftOne.Core.Entities.Subscriptions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Plans;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.Plans;

namespace ShiftOne.Application.Services.Saas
{
    public class SubscriptionPlanService : ISubscriptionPlanService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionPlanService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GeneralResponse> GetAllAsync(PaginationRequest request, bool? isActive)
        {
            var spec = Spec.For<SubscriptionPlan>(isActive.HasValue ? plan => plan.IsActive == isActive : null);
            var countSpec = Spec.For<SubscriptionPlan>(spec.Criteria);
            spec.ApplyOrderBy(plan => plan.Name);
            spec.ApplyPaging((request.Page - 1) * request.PageSize, request.PageSize);
            var plans = (await _unitOfWork.Repository<SubscriptionPlan>().GetAllAsync(spec)).Select(Map).ToList();
            var count = await _unitOfWork.Repository<SubscriptionPlan>().CountAsync(countSpec);
            return GeneralResponse.Ok("Messages.Success", plans, request.Page, request.PageSize, count);
        }

        public async Task<GeneralResponse> GetByIdAsync(Guid id)
        {
            var plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(id);
            return plan == null ? GeneralResponse.NotFound("Messages.NotFound") : GeneralResponse.Ok("Messages.Success", Map(plan));
        }

        public async Task<GeneralResponse> CreateAsync(UpsertSubscriptionPlanRequest request)
        {
            var plan = new SubscriptionPlan();
            Apply(plan, request);
            await _unitOfWork.Repository<SubscriptionPlan>().AddAsync(plan);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success", Map(plan));
        }

        public async Task<GeneralResponse> UpdateAsync(Guid id, UpsertSubscriptionPlanRequest request)
        {
            var plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(id);
            if (plan == null)
            {
                return GeneralResponse.NotFound("Messages.NotFound");
            }

            Apply(plan, request);
            await _unitOfWork.Repository<SubscriptionPlan>().UpdateAsync(plan);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success", Map(plan));
        }

        public async Task<GeneralResponse> DeleteAsync(Guid id)
        {
            var plan = await _unitOfWork.Repository<SubscriptionPlan>().GetByIdAsync(id);
            if (plan == null)
            {
                return GeneralResponse.NotFound("Messages.NotFound");
            }

            plan.IsActive = false;
            await _unitOfWork.Repository<SubscriptionPlan>().UpdateAsync(plan);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success");
        }

        private static void Apply(SubscriptionPlan plan, UpsertSubscriptionPlanRequest request)
        {
            plan.Name = request.Name.Trim();
            plan.Description = request.Description.Trim();
            plan.Price = request.Price;
            plan.BillingPeriod = request.BillingPeriod.Trim();
            plan.IsActive = request.IsActive;
            plan.MaxBranches = NormalizeLimit(request.MaxBranches);
            plan.MaxEmployees = NormalizeLimit(request.MaxEmployees);
            plan.MaxHRUsers = NormalizeLimit(request.MaxHRUsers);
            plan.MaxCompanyAdmins = NormalizeLimit(request.MaxCompanyAdmins);
        }

        private static int? NormalizeLimit(int? value) => value.HasValue && value.Value > 0 ? value.Value : null;

        private static SubscriptionPlanResponse Map(SubscriptionPlan plan) => new()
        {
            Id = plan.Id,
            Name = plan.Name,
            Description = plan.Description,
            Price = plan.Price,
            BillingPeriod = plan.BillingPeriod,
            IsActive = plan.IsActive,
            MaxBranches = plan.MaxBranches,
            MaxEmployees = plan.MaxEmployees,
            MaxHRUsers = plan.MaxHRUsers,
            MaxCompanyAdmins = plan.MaxCompanyAdmins
        };
    }
}
