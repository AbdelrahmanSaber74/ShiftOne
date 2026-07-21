using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.Roles;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.Roles;

namespace ShiftOne.Application.Services.Security
{
    public class RoleManagementService : IRoleManagementService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IUnitOfWork _unitOfWork;

        public RoleManagementService(RoleManager<ApplicationRole> roleManager, IUnitOfWork unitOfWork)
        {
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
        }

        public async Task<GeneralResponse> GetAllAsync(PaginationRequest request, string? keyword, bool? isActive)
        {
            var spec = Spec.ForChain<ApplicationRole>(null, query => query.Include(role => role.RolePermissions).ThenInclude(rolePermission => rolePermission.Permission));
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var clean = keyword.Trim();
                spec.AddCriteria(role => role.Name != null && (role.Name.Contains(clean) || role.Description.Contains(clean)));
            }
            if (isActive.HasValue)
            {
                spec.AddCriteria(role => role.IsActive == isActive.Value);
            }

            var countSpec = Spec.For<ApplicationRole>(spec.Criteria);
            spec.ApplyOrderBy(role => role.Name!);
            spec.ApplyPaging((request.Page - 1) * request.PageSize, request.PageSize);
            var roles = (await _unitOfWork.Repository<ApplicationRole>().GetAllAsync(spec)).Select(Map).ToList();
            var count = await _unitOfWork.Repository<ApplicationRole>().CountAsync(countSpec);
            return GeneralResponse.Ok("Messages.Success", roles, request.Page, request.PageSize, count);
        }

        public async Task<GeneralResponse> GetByIdAsync(Guid id)
        {
            var spec = Spec.ForChain<ApplicationRole>(role => role.Id == id, query => query.Include(role => role.RolePermissions).ThenInclude(rolePermission => rolePermission.Permission));
            var role = (await _unitOfWork.Repository<ApplicationRole>().GetAllAsync(spec)).SingleOrDefault();
            return role == null ? GeneralResponse.NotFound("Messages.NotFound") : GeneralResponse.Ok("Messages.Success", Map(role));
        }

        public async Task<GeneralResponse> CreateAsync(UpsertRoleRequest request)
        {
            var name = request.Name.Trim();
            if (await _roleManager.FindByNameAsync(name) != null)
            {
                return GeneralResponse.BadRequest("Messages.RoleNameInUse");
            }

            var role = new ApplicationRole
            {
                Name = name,
                Description = request.Description.Trim(),
                IsActive = request.IsActive
            };

            var result = await _roleManager.CreateAsync(role);
            return result.Succeeded
                ? GeneralResponse.Ok("Messages.Success", Map(role))
                : GeneralResponse.BadRequest("Messages.InvalidRequest", result.Errors);
        }

        public async Task<GeneralResponse> UpdateAsync(Guid id, UpsertRoleRequest request)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return GeneralResponse.NotFound("Messages.NotFound");
            }

            if (IsSuperAdmin(role) && (!string.Equals(role.Name, request.Name.Trim(), StringComparison.OrdinalIgnoreCase) || !request.IsActive))
            {
                return GeneralResponse.BadRequest("Messages.SuperAdminRoleProtected");
            }

            var newName = request.Name.Trim();
            var existing = await _roleManager.FindByNameAsync(newName);
            if (existing != null && existing.Id != role.Id)
            {
                return GeneralResponse.BadRequest("Messages.RoleNameInUse");
            }

            role.Name = newName;
            role.NormalizedName = _roleManager.NormalizeKey(newName);
            role.Description = request.Description.Trim();
            role.IsActive = request.IsActive;

            var result = await _roleManager.UpdateAsync(role);
            return result.Succeeded
                ? GeneralResponse.Ok("Messages.Success", Map(role))
                : GeneralResponse.BadRequest("Messages.InvalidRequest", result.Errors);
        }

        public async Task<GeneralResponse> DeleteAsync(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return GeneralResponse.NotFound("Messages.NotFound");
            }

            if (IsSystemRole(role.Name))
            {
                return GeneralResponse.BadRequest("Messages.SystemRoleProtected");
            }

            role.IsActive = false;
            var result = await _roleManager.UpdateAsync(role);
            return result.Succeeded
                ? GeneralResponse.Ok("Messages.Success")
                : GeneralResponse.BadRequest("Messages.InvalidRequest", result.Errors);
        }

        public async Task<GeneralResponse> AssignPermissionsAsync(Guid id, AssignRolePermissionsRequest request)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
            {
                return GeneralResponse.NotFound("Messages.NotFound");
            }

            var requestedIds = request.PermissionIds.Distinct().ToHashSet();
            if (IsSuperAdmin(role))
            {
                var allPermissionIds = (await _unitOfWork.Repository<ApplicationPermission>().GetAllAsync())
                    .Where(permission => Permissions.All.Contains(permission.Name))
                    .Select(permission => permission.Id)
                    .ToHashSet();
                if (!allPermissionIds.IsSubsetOf(requestedIds))
                {
                    return GeneralResponse.BadRequest("Messages.SuperAdminPermissionsProtected");
                }
            }

            var permissions = (await _unitOfWork.Repository<ApplicationPermission>().GetAllAsync())
                .Where(permission => requestedIds.Contains(permission.Id))
                .ToList();
            if (permissions.Count != requestedIds.Count)
            {
                return GeneralResponse.BadRequest("Messages.InvalidPermissions");
            }

            var existing = (await _unitOfWork.Repository<ApplicationRolePermission>().GetAllAsync(Spec.For<ApplicationRolePermission>(rolePermission => rolePermission.RoleId == role.Id))).ToList();
            foreach (var rolePermission in existing.Where(rolePermission => !requestedIds.Contains(rolePermission.PermissionId)))
            {
                await _unitOfWork.Repository<ApplicationRolePermission>().DeleteAsync(rolePermission);
            }

            var existingIds = existing.Select(rolePermission => rolePermission.PermissionId).ToHashSet();
            foreach (var permission in permissions.Where(permission => !existingIds.Contains(permission.Id)))
            {
                await _unitOfWork.Repository<ApplicationRolePermission>().AddAsync(new ApplicationRolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }

            await _unitOfWork.CompleteAsync();
            return await GetByIdAsync(role.Id);
        }

        private static bool IsSuperAdmin(ApplicationRole role) => string.Equals(role.Name, Roles.SuperAdmin.ToString(), StringComparison.OrdinalIgnoreCase);

        private static bool IsSystemRole(string? roleName) => Enum.GetNames(typeof(Roles)).Any(role => string.Equals(role, roleName, StringComparison.OrdinalIgnoreCase));

        private static RoleResponse Map(ApplicationRole role) => new()
        {
            Id = role.Id,
            Name = role.Name ?? string.Empty,
            Description = role.Description,
            IsActive = role.IsActive,
            IsSystemRole = IsSystemRole(role.Name),
            IsProtected = string.Equals(role.Name, Roles.SuperAdmin.ToString(), StringComparison.OrdinalIgnoreCase),
            Permissions = role.RolePermissions
                .Select(rolePermission => rolePermission.Permission?.Name)
                .Where(permission => !string.IsNullOrWhiteSpace(permission))
                .Distinct()
                .OrderBy(permission => permission)
                .Select(permission => permission!)
                .ToList(),
            CreatedOn = role.CreatedOn
        };
    }
}