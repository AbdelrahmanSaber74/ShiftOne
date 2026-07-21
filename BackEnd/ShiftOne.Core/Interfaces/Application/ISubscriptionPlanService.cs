using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Plans;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Core.Interfaces.Application
{
    public interface ISubscriptionPlanService
    {
        Task<GeneralResponse> GetAllAsync(PaginationRequest request, bool? isActive);
        Task<GeneralResponse> GetByIdAsync(Guid id);
        Task<GeneralResponse> CreateAsync(UpsertSubscriptionPlanRequest request);
        Task<GeneralResponse> UpdateAsync(Guid id, UpsertSubscriptionPlanRequest request);
        Task<GeneralResponse> DeleteAsync(Guid id);
    }
}
