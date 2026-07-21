using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Permissions;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.Permissions;

namespace ShiftOne.Application.Services.Security
{
    public class PermissionManagementService : IPermissionManagementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PermissionManagementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword)
        {
            var spec = Spec.For<ApplicationPermission>(null);
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var clean = keyword.Trim();
                spec.AddCriteria(permission => permission.Name.Contains(clean) || permission.Description.Contains(clean));
            }

            var countSpec = Spec.For<ApplicationPermission>(spec.Criteria);
            spec.ApplyOrderBy(permission => permission.Name);
            spec.ApplyPaging((request.Page - 1) * request.PageSize, request.PageSize);
            var permissions = (await _unitOfWork.Repository<ApplicationPermission>().GetAllAsync(spec)).Select(Map).ToList();
            var count = await _unitOfWork.Repository<ApplicationPermission>().CountAsync(countSpec);
            return GeneralResponse.Ok("Messages.Success", permissions, request.Page, request.PageSize, count);
        }

        public async Task<GeneralResponse> GetByIdAsync(Guid id)
        {
            var permission = await _unitOfWork.Repository<ApplicationPermission>().GetByIdAsync(id);
            return permission == null ? GeneralResponse.NotFound("Messages.NotFound") : GeneralResponse.Ok("Messages.Success", Map(permission));
        }

        public async Task<GeneralResponse> CreateAsync(UpsertPermissionRequest request)
        {
            var name = request.Name.Trim();
            var exists = await _unitOfWork.Repository<ApplicationPermission>().CountAsync(Spec.For<ApplicationPermission>(permission => permission.Name == name)) > 0;
            if (exists)
            {
                return GeneralResponse.BadRequest("Messages.PermissionNameInUse");
            }

            var permission = new ApplicationPermission
            {
                Name = name,
                Description = request.Description.Trim()
            };
            await _unitOfWork.Repository<ApplicationPermission>().AddAsync(permission);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success", Map(permission));
        }

        public async Task<GeneralResponse> UpdateAsync(Guid id, UpsertPermissionRequest request)
        {
            var permission = await _unitOfWork.Repository<ApplicationPermission>().GetByIdAsync(id);
            if (permission == null)
            {
                return GeneralResponse.NotFound("Messages.NotFound");
            }

            var name = request.Name.Trim();
            if (IsSystemPermission(permission) && !string.Equals(permission.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return GeneralResponse.BadRequest("Messages.SystemPermissionProtected");
            }

            var exists = await _unitOfWork.Repository<ApplicationPermission>().CountAsync(Spec.For<ApplicationPermission>(item => item.Name == name && item.Id != id)) > 0;
            if (exists)
            {
                return GeneralResponse.BadRequest("Messages.PermissionNameInUse");
            }

            permission.Name = name;
            permission.Description = request.Description.Trim();
            await _unitOfWork.Repository<ApplicationPermission>().UpdateAsync(permission);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success", Map(permission));
        }

        public async Task<GeneralResponse> DeleteAsync(Guid id)
        {
            var permission = await _unitOfWork.Repository<ApplicationPermission>().GetByIdAsync(id);
            if (permission == null)
            {
                return GeneralResponse.NotFound("Messages.NotFound");
            }

            if (IsSystemPermission(permission))
            {
                return GeneralResponse.BadRequest("Messages.SystemPermissionProtected");
            }

            var rolePermissions = (await _unitOfWork.Repository<ApplicationRolePermission>().GetAllAsync(Spec.For<ApplicationRolePermission>(rolePermission => rolePermission.PermissionId == id))).ToList();
            foreach (var rolePermission in rolePermissions)
            {
                await _unitOfWork.Repository<ApplicationRolePermission>().DeleteAsync(rolePermission);
            }

            await _unitOfWork.Repository<ApplicationPermission>().DeleteAsync(permission);
            await _unitOfWork.CompleteAsync();
            return GeneralResponse.Ok("Messages.Success");
        }

        private static bool IsSystemPermission(ApplicationPermission permission) => Permissions.All.Contains(permission.Name);

        private static PermissionResponse Map(ApplicationPermission permission) => new()
        {
            Id = permission.Id,
            Name = permission.Name,
            Description = permission.Description,
            IsSystemPermission = IsSystemPermission(permission),
            CreatedOn = permission.CreatedOn
        };
    }
}