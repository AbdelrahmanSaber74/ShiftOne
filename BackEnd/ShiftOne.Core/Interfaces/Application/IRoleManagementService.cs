using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Roles;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Core.Interfaces.Application
{
    public interface IRoleManagementService
    {
        Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword, bool? isActive);
        Task<GeneralResponse> GetByIdAsync(Guid id);
        Task<GeneralResponse> CreateAsync(UpsertRoleRequest request);
        Task<GeneralResponse> UpdateAsync(Guid id, UpsertRoleRequest request);
        Task<GeneralResponse> DeleteAsync(Guid id);
        Task<GeneralResponse> AssignPermissionsAsync(Guid id, AssignRolePermissionsRequest request);
    }
}