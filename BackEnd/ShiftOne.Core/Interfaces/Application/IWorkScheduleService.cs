using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.WorkSchedules;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Core.Interfaces.Application
{
    public interface IWorkScheduleService
    {
        Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword, Guid? companyId, Guid? branchId, bool? isActive);
        Task<GeneralResponse> GetByIdAsync(Guid id);
        Task<GeneralResponse> CreateAsync(UpsertWorkScheduleRequest request);
        Task<GeneralResponse> UpdateAsync(Guid id, UpsertWorkScheduleRequest request);
        Task<GeneralResponse> DeleteAsync(Guid id);
        Task<GeneralResponse> SetDefaultAsync(Guid id);
        Task<GeneralResponse> AssignToBranchAsync(Guid branchId, AssignBranchScheduleRequest request);
        Task<GeneralResponse> ClearBranchScheduleAsync(Guid branchId);
    }
}
