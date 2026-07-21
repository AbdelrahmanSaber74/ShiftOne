namespace ShiftOne.Core.Interfaces.Application
{
    public interface IPlanLimitService
    {
        Task<bool> CanCreateBranchAsync(Guid companyId);
        Task<bool> CanCreateEmployeeAsync(Guid companyId);
        Task<bool> CanCreateHrAsync(Guid companyId);
        Task<bool> CanCreateCompanyAdminAsync(Guid companyId);
    }
}
