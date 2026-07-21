using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Identity;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.User;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.User;

namespace ShiftOne.Application.Services.User
{
    public partial class UserService
    {
        public async Task<GeneralResponse> ApproveUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return GeneralResponse.NotFound("Messages.UserNotFound");

            if (user.IsActive)
                return GeneralResponse.BadRequest("Messages.UserAlreadyApproved");

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return GeneralResponse.BadRequest("Messages.UserApprovalFailed", result.Errors);

            return GeneralResponse.Ok("Messages.UserApproved");
        }

        public async Task<GeneralResponse> UnApproveUserAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return GeneralResponse.NotFound("Messages.UserNotFound");

            if (await IsProtectedSuperAdminAsync(user))
                return GeneralResponse.BadRequest("Messages.SuperAdminUserProtected");

            if (!user.IsActive)
                return GeneralResponse.BadRequest("Messages.UserAlreadyUnapproved");

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return GeneralResponse.BadRequest("Messages.UserUnapprovalFailed", result.Errors);

            return GeneralResponse.Ok("Messages.UserUnapproved");
        }

        public async Task<GeneralResponse> AdminResetUserPasswordAsync(AdminResetUserPasswordRequest request)
        {
            var currentAdminId = _currentUserService.CurrentUserId;
            if (!currentAdminId.HasValue)
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            var currentAdmin = await _userManager.FindByIdAsync(currentAdminId.Value.ToString());
            if (currentAdmin == null ||
                !currentAdmin.IsActive ||
                !await IsUserAllowedDashboardAccessAsync(currentAdmin))
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            var user = await _userManager.FindByIdAsync(request.userId.ToString());
            if (user == null)
            {
                return GeneralResponse.NotFound("Messages.UserNotFound");
            }

            if (await IsProtectedSuperAdminAsync(user))
            {
                return GeneralResponse.BadRequest("Messages.SuperAdminUserProtected");
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetResult = await _userManager.ResetPasswordAsync(user, resetToken, request.newPassword);
            if (!resetResult.Succeeded)
            {
                return GeneralResponse.BadRequest("Messages.PasswordResetFailed", resetResult.Errors);
            }

            await _userManager.UpdateSecurityStampAsync(user);
            await RevokeActiveRefreshTokensAsync(user.Id, "Password reset by admin.");

            return GeneralResponse.Ok("Messages.UserPasswordReset");
        }

        public async Task<GeneralResponse> AdminActivateUserEmailAsync(AdminActivateUserEmailRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.userId.ToString());
            if (user == null)
            {
                return GeneralResponse.NotFound("Messages.UserNotFound");
            }

            if (string.IsNullOrWhiteSpace(user.Email))
            {
                return GeneralResponse.BadRequest("Messages.UserNoEmail");
            }

            if (user.EmailConfirmed)
            {
                return GeneralResponse.Ok("Messages.EmailAlreadyVerified");
            }

            user.EmailConfirmed = true;
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                return GeneralResponse.BadRequest("Messages.EmailVerificationFailed", result.Errors);
            }

            return GeneralResponse.Ok("Messages.EmailVerified");
        }

        public async Task<GeneralResponse> GetAllUsersAsync(PaginationRequest paginationRequest, string? keyword, bool? isActive)
        {
            var spec = Spec.For<Customer>(null);

            if (!string.IsNullOrEmpty(keyword))
            {
                keyword = keyword.Replace(" ", "").Replace("+", "").Replace("%20", "");
                spec.AddCriteria(u => u.FirstName.Contains(keyword)
                            || u.LastName.Contains(keyword)
                            || (u.Email != null && u.Email.Contains(keyword))
                            || (u.PhoneNumber != null && u.PhoneNumber.Contains(keyword)));
            }

            if (isActive.HasValue)
            {
                spec.AddCriteria(u => u.IsActive == isActive);
            }
            var countSpec = Spec.For<Customer>(spec.Criteria);

            int skip = (paginationRequest.Page - 1) * paginationRequest.PageSize;
            spec.ApplyPaging(skip, paginationRequest.PageSize);
            spec.ApplyOrderByDescending(u => u.CreatedOn);
            var users = await _unitOfWork.Repository<Customer>().GetAllAsync(spec);
            var userCount = await _unitOfWork.Repository<Customer>().CountAsync(countSpec);
            var data = new GetAllUsersResponse();
            foreach (var userData in users)
            {
                if (await _userManager.IsInRoleAsync(userData, Roles.Admin.ToString()))
                {
                    userCount--;
                    continue;
                }
                var user = new GetUserByIdResponse()
                {
                    CreatedOn = userData.CreatedOn,
                    Email = userData.Email ?? string.Empty,
                    EmailConfirmed = userData.EmailConfirmed,
                    PhoneConfirmed = userData.PhoneNumberConfirmed,
                    FirstName = userData.FirstName,
                    LastName = userData.LastName,
                    Id = userData.Id,
                    IsActive = userData.IsActive,
                    PhoneNumber = userData.PhoneNumber ?? string.Empty
                };
                var imagePath = (await _fileService.GetFileUrlAsync(userData.ImagePath));
                user.ImagePath = imagePath != null ? imagePath : null;
                var userRoles = await _userManager.GetRolesAsync(userData);
                if (userRoles.Count > 0)
                    user.Roles.AddRange(userRoles);
                user.IsProtected = userRoles.Any(role => string.Equals(role, Roles.SuperAdmin.ToString(), StringComparison.OrdinalIgnoreCase));
                data.Users.Add(user);
            }
            return GeneralResponse.Ok("Messages.GetUsersSuccess", data,
                page: paginationRequest.Page,
                pageSize: paginationRequest.PageSize,
                totalCount: userCount);
        }

        public async Task<GeneralResponse> AdminGetUserByIdAsync(Guid userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return GeneralResponse.NotFound("Messages.UserNotFound");
            var data = new GetUserByIdResponse()
            {
                CreatedOn = user.CreatedOn,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Id = user.Id,
                IsActive = user.IsActive,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
            };
            var imagePath = (await _fileService.GetFileUrlAsync(user.ImagePath));
            data.ImagePath = imagePath != null ? imagePath : null;
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Count > 0)
                data.Roles.AddRange(userRoles);
            data.IsProtected = userRoles.Any(role => string.Equals(role, Roles.SuperAdmin.ToString(), StringComparison.OrdinalIgnoreCase));
            return GeneralResponse.Ok("Messages.GetUserDataSuccess", data);
        }

        private async Task<bool> IsProtectedSuperAdminAsync(ApplicationUser user)
        {
            return await _userManager.IsInRoleAsync(user, Roles.SuperAdmin.ToString());
        }

        public async Task<GeneralResponse> GetCurrentAdminContextAsync()
        {
            var currentUserId = _currentUserService.CurrentUserId;
            if (!currentUserId.HasValue)
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            var user = await _userManager.FindByIdAsync(currentUserId.Value.ToString());
            if (user == null)
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            if (!user.IsActive || !await IsUserAllowedDashboardAccessAsync(user))
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var activeRoleNames = userRoles
                .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                .Distinct()
                .ToList();

            var rolePermissionSpec = Spec.ForChain<ApplicationRolePermission>(
                rolePermission => rolePermission.Role.IsActive &&
                                  rolePermission.Role.Name != null &&
                                  activeRoleNames.Contains(rolePermission.Role.Name),
                query => query.Include(rolePermission => rolePermission.Role),
                query => query.Include(rolePermission => rolePermission.Permission));

            var rolePermissions = await _unitOfWork.Repository<ApplicationRolePermission>().GetAllAsync(rolePermissionSpec);
            var permissions = rolePermissions
                .Select(rolePermission => rolePermission.Permission.Name)
                .Where(permission => !string.IsNullOrWhiteSpace(permission))
                .Distinct()
                .OrderBy(permission => permission)
                .ToList();

            var data = new AdminCurrentUserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                PhoneConfirmed = user.PhoneNumberConfirmed,
                ImagePath = await _fileService.GetFileUrlAsync(user.ImagePath),
                IsActive = user.IsActive,
                IsLockedOut = await _userManager.IsLockedOutAsync(user),
                LockoutEnd = user.LockoutEnd,
                CreatedOn = user.CreatedOn,
                Roles = activeRoleNames.OrderBy(role => role).ToList(),
                Permissions = permissions
            };

            return GeneralResponse.Ok("Messages.GetAdminInfoSuccess", data);
        }
    }
}