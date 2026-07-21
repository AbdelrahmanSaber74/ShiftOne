using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Companies;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Core.Interfaces.Application
{
    public interface ICompanyService
    {
        Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword, bool? isActive);
        Task<GeneralResponse> GetByIdAsync(Guid id);
        Task<GeneralResponse> CreateAsync(CreateCompanyRequest request);
        Task<GeneralResponse> UpdateAsync(Guid id, UpdateCompanyRequest request);
        Task<GeneralResponse> DeleteAsync(Guid id);
    }
}
