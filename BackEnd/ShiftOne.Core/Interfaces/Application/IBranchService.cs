using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Branches;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Core.Interfaces.Application
{
    public interface IBranchService
    {
        Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword, bool? isActive, Guid? companyId = null);
        Task<GeneralResponse> GetByIdAsync(Guid id);
        Task<GeneralResponse> CreateAsync(UpsertBranchRequest request);
        Task<GeneralResponse> UpdateAsync(Guid id, UpsertBranchRequest request);
        Task<GeneralResponse> DeleteAsync(Guid id);
    }
}
