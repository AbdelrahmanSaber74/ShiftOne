using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Employees;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Core.Interfaces.Application
{
    public interface IEmployeeManagementService
    {
        Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword, bool? isActive, Guid? branchId);
        Task<GeneralResponse> GetByIdAsync(Guid id);
        Task<GeneralResponse> CreateAsync(CreateEmployeeRequest request);
        Task<GeneralResponse> UpdateAsync(Guid id, UpdateEmployeeRequest request);
        Task<GeneralResponse> DeleteAsync(Guid id);
        Task<GeneralResponse> ResetDeviceAsync(Guid employeeId);
    }
}
