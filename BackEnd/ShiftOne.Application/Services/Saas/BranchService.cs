using ShiftOne.Core.Entities.Branches;
using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Companies;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Branches;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.Branches;

namespace ShiftOne.Application.Services.Saas
{
    public class BranchService : IBranchService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITenantContext _tenantContext;
        private readonly IPlanLimitService _planLimitService;

        public BranchService(IUnitOfWork unitOfWork, ITenantContext tenantContext, IPlanLimitService planLimitService)
        {
            _unitOfWork = unitOfWork;
            _tenantContext = tenantContext;
            _planLimitService = planLimitService;
        }

        public async Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword, bool? isActive, Guid? companyId = null)
        {
            var resolvedCompanyId = _tenantContext.ResolveCompanyId(companyId);
            Specification<Branch> spec;

            if (resolvedCompanyId.HasValue)
            {
                spec = Spec.ForChain<Branch>(branch => branch.CompanyId == resolvedCompanyId.Value, query => query.Include(branch => branch.WorkSchedule));
            }
            else if (_tenantContext.IsPlatformAdmin)
            {
                spec = Spec.ForChain<Branch>(branch => true, query => query.Include(branch => branch.WorkSchedule));
            }
            else
            {
                return GeneralResponse.BadRequest("Messages.CompanyContextRequired");
            }
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var clean = keyword.Trim();
                spec.AddCriteria(branch => branch.Name.Contains(clean) || branch.Code.Contains(clean));
            }
            if (isActive.HasValue)
            {
                spec.AddCriteria(branch => branch.IsActive == isActive.Value);
            }

            var countSpec = Spec.For<Branch>(spec.Criteria);
            spec.ApplyOrderBy(branch => branch.Name);
            spec.ApplyPaging((request.Page - 1) * request.PageSize, request.PageSize);
            var data = (await _unitOfWork.Repository<Branch>().GetAllAsync(spec)).Select(Map).ToList();
            var count = await _unitOfWork.Repository<Branch>().CountAsync(countSpec);
            return GeneralResponse.Ok("Messages.Success", data, request.Page, request.PageSize, count);
        }

        public async Task<GeneralResponse> GetByIdAsync(Guid id)
        {
            var branch = await GetBranchForReadAsync(id);
            return branch == null ? GeneralResponse.NotFound("Messages.NotFound") : GeneralResponse.Ok("Messages.Success", Map(branch));
        }

        public async Task<GeneralResponse> CreateAsync(UpsertBranchRequest request)
        {
            Guid? companyId;
            try
            {
                companyId = _tenantContext.ResolveCompanyId(request.CompanyId);
            }
            catch (UnauthorizedAccessException)
            {
                return GeneralResponse.Unauthorized("Messages.CompanyContextRequired");
            }

            if (!companyId.HasValue)
            {
                return GeneralResponse.BadRequest("Messages.CompanyContextRequired");
            }

            if (!await IsActiveCompanyAsync(companyId.Value))
            {
                return GeneralResponse.BadRequest("Messages.CompanyContextRequired");
            }

            if (!await _planLimitService.CanCreateBranchAsync(companyId.Value))
            {
                return GeneralResponse.BadRequest("Messages.PlanLimitExceeded");
            }

            var existingCount = await _unitOfWork.Repository<Branch>().CountAsync(TenantSpecifications.ForCompany<Branch>(companyId.Value));
            var isMain = request.IsMainBranch || existingCount == 0;
            if (isMain)
            {
                await ClearMainBranchesAsync(companyId.Value);
            }

            var branch = new Branch
            {
                CompanyId = companyId.Value,
                Name = request.Name.Trim(),
                Code = request.Code.Trim(),
                Address = request.Address.Trim(),
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                AllowedRadius = request.AllowedRadius,
                IsMainBranch = isMain,
                IsActive = request.IsActive,
                WorkScheduleId = request.WorkScheduleId
            };

            await _unitOfWork.Repository<Branch>().AddAsync(branch);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success", Map(branch));
        }

        public async Task<GeneralResponse> UpdateAsync(Guid id, UpsertBranchRequest request)
        {
            var branch = await GetBranchForMutationAsync(id);
            if (branch == null)
            {
                return GeneralResponse.NotFound("Messages.NotFound");
            }

            var targetCompanyId = branch.CompanyId;
            if (_tenantContext.IsPlatformAdmin && request.CompanyId.HasValue && request.CompanyId.Value != branch.CompanyId)
            {
                targetCompanyId = request.CompanyId.Value;
                if (!await IsActiveCompanyAsync(targetCompanyId))
                {
                    return GeneralResponse.BadRequest("Messages.CompanyContextRequired");
                }

                if (!await _planLimitService.CanCreateBranchAsync(targetCompanyId))
                {
                    return GeneralResponse.BadRequest("Messages.PlanLimitExceeded");
                }
            }

            if (!_tenantContext.CanAccessCompany(branch.CompanyId) || !_tenantContext.CanAccessCompany(targetCompanyId))
            {
                return GeneralResponse.Unauthorized("Messages.CompanyContextRequired");
            }

            if (request.IsMainBranch && (!branch.IsMainBranch || targetCompanyId != branch.CompanyId))
            {
                await ClearMainBranchesAsync(targetCompanyId);
                branch = await GetBranchByIdAsync(id) ?? branch;
            }

            branch.CompanyId = targetCompanyId;
            branch.Name = request.Name.Trim();
            branch.Code = request.Code.Trim();
            branch.Address = request.Address.Trim();
            branch.Latitude = request.Latitude;
            branch.Longitude = request.Longitude;
            branch.AllowedRadius = request.AllowedRadius;
            branch.IsMainBranch = request.IsMainBranch || branch.IsMainBranch;
            branch.IsActive = request.IsActive;
            branch.WorkScheduleId = request.WorkScheduleId;
            await _unitOfWork.Repository<Branch>().UpdateAsync(branch);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success", Map(branch));
        }

        public async Task<GeneralResponse> DeleteAsync(Guid id)
        {
            var branch = await GetBranchForMutationAsync(id);
            if (branch == null)
            {
                return GeneralResponse.NotFound("Messages.NotFound");
            }

            branch.IsActive = false;
            await _unitOfWork.Repository<Branch>().UpdateAsync(branch);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success");
        }

        private async Task<bool> IsActiveCompanyAsync(Guid companyId)
        {
            var company = await _unitOfWork.Repository<Company>().GetByIdAsync(companyId);
            return company is { IsActive: true };
        }

        private async Task ClearMainBranchesAsync(Guid companyId)
        {
            var branches = await _unitOfWork.Repository<Branch>().GetAllAsync(Spec.For<Branch>(branch => branch.CompanyId == companyId && branch.IsMainBranch));
            foreach (var branch in branches)
            {
                branch.IsMainBranch = false;
                await _unitOfWork.Repository<Branch>().UpdateAsync(branch);
            }
            await _unitOfWork.CompleteAsync();
        }

        private async Task<Branch?> GetBranchForReadAsync(Guid id)
        {
            return _tenantContext.IsPlatformAdmin ? await GetBranchByIdAsync(id) : await GetTenantBranchAsync(id);
        }

        private async Task<Branch?> GetBranchForMutationAsync(Guid id)
        {
            return _tenantContext.IsPlatformAdmin ? await GetBranchByIdAsync(id) : await GetTenantBranchAsync(id);
        }

        private async Task<Branch?> GetBranchByIdAsync(Guid id)
        {
            var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(id);
            return branch != null && _tenantContext.CanAccessCompany(branch.CompanyId) ? branch : null;
        }

        private async Task<Branch?> GetTenantBranchAsync(Guid id)
        {
            if (!_tenantContext.CompanyId.HasValue)
            {
                return null;
            }

            return (await _unitOfWork.Repository<Branch>().GetAllAsync(Spec.ForChain<Branch>(branch => branch.Id == id && branch.CompanyId == _tenantContext.CompanyId.Value, query => query.Include(branch => branch.WorkSchedule)))).SingleOrDefault();
        }

        private static BranchResponse Map(Branch branch) => new()
        {
            Id = branch.Id,
            CompanyId = branch.CompanyId,
            Name = branch.Name,
            Code = branch.Code,
            Address = branch.Address,
            Latitude = branch.Latitude,
            Longitude = branch.Longitude,
            AllowedRadius = branch.AllowedRadius,
            IsMainBranch = branch.IsMainBranch,
            IsActive = branch.IsActive,
            WorkScheduleId = branch.WorkScheduleId,
            WorkScheduleName = branch.WorkSchedule?.Name
        };
    }
}