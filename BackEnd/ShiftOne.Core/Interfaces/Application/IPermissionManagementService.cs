using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Permissions;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Core.Interfaces.Application
{
    public interface IPermissionManagementService
    {
        Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword);
        Task<GeneralResponse> GetByIdAsync(Guid id);
        Task<GeneralResponse> CreateAsync(UpsertPermissionRequest request);
        Task<GeneralResponse> UpdateAsync(Guid id, UpsertPermissionRequest request);
        Task<GeneralResponse> DeleteAsync(Guid id);
    }
}