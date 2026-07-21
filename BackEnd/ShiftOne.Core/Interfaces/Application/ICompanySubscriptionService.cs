using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Subscriptions;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Core.Interfaces.Application
{
    public interface ICompanySubscriptionService
    {
        Task<GeneralResponse> GetAllAsync(PaginationRequest request, Guid? companyId);
        Task<GeneralResponse> AssignAsync(AssignCompanySubscriptionRequest request);
    }
}
