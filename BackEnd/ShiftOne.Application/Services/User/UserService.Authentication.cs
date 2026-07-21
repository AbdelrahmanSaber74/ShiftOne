using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Common.Constants;
using ShiftOne.Core.Entities.Identity;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Specifications;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests.User;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.User;
using ShiftOne.Shared.Validation;

namespace ShiftOne.Application.Services.User
{
    public partial class UserService
    {
        public async Task<GeneralResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            var identifier = EmailOrPhoneValidator.Normalize(registerRequest.emailOrPhone);
            var identifierKind = EmailOrPhoneValidator.GetKind(identifier);

            if (identifierKind == EmailOrPhoneKind.Invalid)
            {
                return GeneralResponse.BadRequest("Messages.InvalidRequest");
            }

            if (identifierKind == EmailOrPhoneKind.Email &&
                await _userManager.FindByEmailAsync(identifier) != null)
            {
                return GeneralResponse.BadRequest("Messages.EmailInUse");
            }

            if (identifierKind == EmailOrPhoneKind.Phone &&
                await _userManager.Users.AnyAsync(user => user.PhoneNumber == identifier))
            {
                return GeneralResponse.BadRequest("Messages.PhoneInUse");
            }

            var user = new Customer
            {
                Email = identifierKind == EmailOrPhoneKind.Email ? identifier : null,
                PhoneNumber = identifierKind == EmailOrPhoneKind.Phone ? identifier : null,
                EmailConfirmed = false,
                FirstName = registerRequest.firstName.Trim(),
                LastName = registerRequest.lastName.Trim(),
                UserName = Guid.NewGuid().ToString(),
                IsActive = true,
                PhoneNumberConfirmed = false,
            };

            var result = await _userManager.CreateAsync(user, registerRequest.password);
            if (!result.Succeeded)
            {
                return GeneralResponse.BadRequest("Messages.InvalidRequest", result.Errors);
            }

            await _userManager.AddToRoleAsync(user, Roles.Customer.ToString());
            if (registerRequest.Picture != null)
            {
                try
                {
                    var imagePath = await _fileService.UploadImageAsync(FilePathType.UserProfiles, user.Id, registerRequest.Picture);
                    user.ImagePath = imagePath;

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (!updateResult.Succeeded)
                    {
                        await _fileService.DeleteFileAsync(imagePath);
                        user.ImagePath = null;
                        return GeneralResponse.BadRequest("Messages.OperationFailed", updateResult.Errors);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    user.ImagePath = null;
                    return GeneralResponse.BadRequest("Messages.OperationFailed", new { Error = ex.Message });
                }
                catch (Exception ex)
                {
                    user.ImagePath = null;
                    return GeneralResponse.InternalError("Messages.OperationFailed", new { Error = ex.Message });
                }
            }

            var verificationTarget = identifierKind == EmailOrPhoneKind.Email ? "email" : "phone";
            return new GeneralResponse("Messages.UserCreatedVerification", true, 200)
            {
                MessagePlaceholders = new() { { "Target", verificationTarget } }
            };
        }

        public async Task<GeneralResponse> LoginAsync(LoginRequest loginRequest)
        {
            if (EmailOrPhoneValidator.GetKind(loginRequest.emailOrPhone) == EmailOrPhoneKind.Invalid)
            {
                return GeneralResponse.BadRequest("Messages.InvalidRequest");
            }

            var user = await FindUserByEmailOrPhoneAsync(loginRequest.emailOrPhone);
            return await LoginUserAsync(user, loginRequest.password, requireAdmin: false, touchCustomer: true, loginRequest.deviceId);
        }

        public async Task<GeneralResponse> AdminLoginAsync(LoginRequest loginRequest)
        {
            if (EmailOrPhoneValidator.GetKind(loginRequest.emailOrPhone) == EmailOrPhoneKind.Invalid)
            {
                return GeneralResponse.BadRequest("Messages.InvalidRequest");
            }

            var user = await FindUserByEmailOrPhoneAsync(loginRequest.emailOrPhone);
            return await LoginUserAsync(user, loginRequest.password, requireAdmin: true, touchCustomer: false, loginRequest.deviceId);
        }

        public async Task<GeneralResponse> RefreshTokenAsync(string refreshToken)
        {
            return await RefreshAccessTokenAsync(refreshToken, requireAdmin: false);
        }

        public async Task<GeneralResponse> AdminRefreshTokenAsync(string refreshToken)
        {
            return await RefreshAccessTokenAsync(refreshToken, requireAdmin: true);
        }

        public async Task<GeneralResponse> RevokeTokenAsync(string refreshToken)
        {
            var savedToken = await FindRefreshTokenAsync(refreshToken);
            if (savedToken == null || !savedToken.IsActive)
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            savedToken.IsRevoked = true;
            savedToken.RevokedOn = DateTime.UtcNow;
            savedToken.RevokedByIp = GetRequestIpAddress();
            savedToken.ReasonRevoked = "Revoked by user.";
            await _unitOfWork.Repository<RefreshToken>().UpdateAsync(savedToken);
            await _unitOfWork.CompleteAsync();

            return GeneralResponse.Ok("Messages.LogoutSuccess");
        }

        private async Task<GeneralResponse> LoginUserAsync(
            ApplicationUser? user,
            string password,
            bool requireAdmin,
            bool touchCustomer,
            string? deviceId)
        {
            if (user == null)
            {
                await DelayInvalidLoginAsync();
                return GeneralResponse.Unauthorized("Messages.InvalidEmailPhoneOrPassword");
            }

            if (requireAdmin && !await IsUserAllowedDashboardAccessAsync(user))
            {
                await DelayInvalidLoginAsync();
                return GeneralResponse.Unauthorized("Messages.InvalidEmailPhoneOrPassword");
            }

            if (await _userManager.IsLockedOutAsync(user))
            {
                return GeneralResponse.Unauthorized("Messages.InvalidEmailPhoneOrPassword");
            }

            if (!user.IsActive)
            {
                return GeneralResponse.Unauthorized("Messages.InvalidEmailPhoneOrPassword");
            }

            if (!await _userManager.CheckPasswordAsync(user, password))
            {
                await _userManager.AccessFailedAsync(user);
                await DelayInvalidLoginAsync();
                return GeneralResponse.Unauthorized("Messages.InvalidEmailPhoneOrPassword");
            }

            if (!requireAdmin && !IsUserConfirmedForLogin(user))
            {
                return GeneralResponse.Unauthorized("Messages.VerifyAccount");
            }

            if (!requireAdmin && await IsUserInActiveRoleAsync(user, Roles.Employee.ToString()))
            {
                var deviceResult = await EnsureEmployeeDeviceAsync(user, deviceId);
                if (!deviceResult.Success)
                {
                    return deviceResult;
                }
            }

            await _userManager.ResetAccessFailedCountAsync(user);
            var data = await IssueLoginTokensAsync(user);

            if (touchCustomer)
            {
                var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(user.Id);
                if (customer != null)
                {
                    await _unitOfWork.Repository<Customer>().UpdateAsync(customer);
                    await _unitOfWork.CompleteAsync();
                }
            }

            return GeneralResponse.Ok("Messages.LoginSuccess", data);
        }

        private async Task<ApplicationUser?> FindUserByEmailOrPhoneAsync(string emailOrPhone)
        {
            var identifier = EmailOrPhoneValidator.Normalize(emailOrPhone);
            var identifierKind = EmailOrPhoneValidator.GetKind(identifier);

            return identifierKind switch
            {
                EmailOrPhoneKind.Email => await _userManager.FindByEmailAsync(identifier),
                EmailOrPhoneKind.Phone => await _userManager.Users.FirstOrDefaultAsync(user => user.PhoneNumber == identifier),
                _ => null
            };
        }

        private async Task<LoginResponse> IssueLoginTokensAsync(ApplicationUser user)
        {
            var accessToken = await _jwtService.GenerateJwtToken(user, _userManager);
            var refreshToken = await _jwtService.GenerateRefreshToken();
            await _jwtService.SaveRefreshToken(user.Id, refreshToken, GetRequestIpAddress());

            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        private async Task<GeneralResponse> RefreshAccessTokenAsync(string refreshToken, bool requireAdmin)
        {
            var savedToken = await FindRefreshTokenAsync(refreshToken);
            if (savedToken == null)
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            if (!savedToken.IsActive)
            {
                await RevokeActiveRefreshTokensAsync(savedToken.ApplicationUserId, "Refresh token reuse detected.");
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            var user = await _userManager.FindByIdAsync(savedToken.ApplicationUserId.ToString());
            if (user == null)
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            if (requireAdmin && !await IsUserAllowedDashboardAccessAsync(user))
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            if (await _userManager.IsLockedOutAsync(user) ||
                !user.IsActive ||
                (!requireAdmin && !IsUserConfirmedForLogin(user)))
            {
                savedToken.IsRevoked = true;
                savedToken.RevokedOn = DateTime.UtcNow;
                savedToken.RevokedByIp = GetRequestIpAddress();
                savedToken.ReasonRevoked = "User is not allowed to refresh token.";
                await _unitOfWork.CompleteAsync();
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            var newRefreshToken = await _jwtService.GenerateRefreshToken();
            savedToken.IsRevoked = true;
            savedToken.RevokedOn = DateTime.UtcNow;
            savedToken.RevokedByIp = GetRequestIpAddress();
            savedToken.ReplacedByTokenHash = _jwtService.HashRefreshToken(newRefreshToken);
            savedToken.ReasonRevoked = "Replaced by refresh token rotation.";

            await _jwtService.SaveRefreshToken(user.Id, newRefreshToken, GetRequestIpAddress());

            var data = new RefreshTokenResponse
            {
                AccessToken = await _jwtService.GenerateJwtToken(user, _userManager),
                RefreshToken = newRefreshToken
            };

            return GeneralResponse.Ok("Messages.Success", data);
        }

        private async Task<RefreshToken?> FindRefreshTokenAsync(string refreshToken)
        {
            var tokenHash = _jwtService.HashRefreshToken(refreshToken);
            var spec = Spec.For<RefreshToken>(token => token.TokenHash == tokenHash);
            var tokens = await _unitOfWork.Repository<RefreshToken>().GetAllAsync(spec);

            return tokens.SingleOrDefault();
        }

        private static bool IsUserConfirmedForLogin(ApplicationUser user)
        {
            if (!string.IsNullOrWhiteSpace(user.Email))
            {
                return user.EmailConfirmed;
            }

            if (!string.IsNullOrWhiteSpace(user.PhoneNumber))
            {
                return user.PhoneNumberConfirmed;
            }

            return false;
        }

        private async Task<bool> IsUserInActiveRoleAsync(ApplicationUser user, string roleName)
        {
            if (!await _userManager.IsInRoleAsync(user, roleName))
            {
                return false;
            }

            var spec = Spec.For<ApplicationRole>(role =>
                role.Name == roleName &&
                role.IsActive);
            var roles = await _unitOfWork.Repository<ApplicationRole>().GetAllAsync(spec);

            return roles.Any();
        }

        private async Task RevokeActiveRefreshTokensAsync(Guid userId, string reason)
        {
            var spec = Spec.For<RefreshToken>(token => token.ApplicationUserId == userId && !token.IsRevoked);
            var activeTokens = await _unitOfWork.Repository<RefreshToken>().GetAllAsync(spec);

            foreach (var token in activeTokens.Where(token => token.IsActive))
            {
                token.IsRevoked = true;
                token.RevokedOn = DateTime.UtcNow;
                token.RevokedByIp = GetRequestIpAddress();
                token.ReasonRevoked = reason;
                await _unitOfWork.Repository<RefreshToken>().UpdateAsync(token);
            }

            await _unitOfWork.CompleteAsync();
        }


        private async Task<bool> IsUserAllowedDashboardAccessAsync(ApplicationUser user)
        {
            return await IsUserInActiveRoleAsync(user, Roles.SuperAdmin.ToString()) ||
                   await IsUserInActiveRoleAsync(user, Roles.Admin.ToString()) ||
                   await IsUserInActiveRoleAsync(user, Roles.CompanyAdmin.ToString()) ||
                   await IsUserInActiveRoleAsync(user, Roles.HR.ToString());
        }

        private async Task<GeneralResponse> EnsureEmployeeDeviceAsync(ApplicationUser user, string? deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId))
            {
                return GeneralResponse.BadRequest("Messages.DeviceRequired");
            }

            var spec = Spec.For<EmployeeDevice>(device => device.EmployeeId == user.Id && device.IsActive);
            var activeDevice = (await _unitOfWork.Repository<EmployeeDevice>().GetAllAsync(spec)).SingleOrDefault();
            if (activeDevice == null)
            {
                await _unitOfWork.Repository<EmployeeDevice>().AddAsync(new EmployeeDevice
                {
                    EmployeeId = user.Id,
                    DeviceId = deviceId.Trim(),
                    BoundOn = DateTime.UtcNow,
                    IsActive = true
                });
                await _unitOfWork.CompleteAsync();
                return GeneralResponse.Ok();
            }

            if (!string.Equals(activeDevice.DeviceId, deviceId.Trim(), StringComparison.Ordinal))
            {
                return GeneralResponse.Unauthorized("Messages.DeviceMismatch");
            }

            return GeneralResponse.Ok();
        }
        private string GetRequestIpAddress()
        {
            return _currentUserService.CurrentIpAddress ?? string.Empty;
        }

        private static Task DelayInvalidLoginAsync()
        {
            return Task.Delay(TimeSpan.FromMilliseconds(AppConstants.Auth.LoginDelayMs));
        }
    }
}


