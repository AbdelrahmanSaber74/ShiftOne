using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Identity;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests.User;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.User;
using ShiftOne.Shared.Validation;

namespace ShiftOne.Application.Services.User
{
    public partial class UserService
    {
        public async Task<GeneralResponse> EditProfileAsync(EditProfileRequest editProfileRequest)
        {
            var userId = _currentUserService.CurrentUserId;
            if (!userId.HasValue)
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            if (string.IsNullOrWhiteSpace(editProfileRequest.firstName) &&
                string.IsNullOrWhiteSpace(editProfileRequest.lastName) &&
                string.IsNullOrWhiteSpace(editProfileRequest.email) &&
                string.IsNullOrWhiteSpace(editProfileRequest.phone) &&
                editProfileRequest.Picture == null)
            {
                return GeneralResponse.BadRequest("Messages.InvalidProfileFields");
            }

            var user = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(userId.Value);
            if (user == null)
                return GeneralResponse.NotFound("Messages.UserNotFound");

            if (!string.IsNullOrWhiteSpace(editProfileRequest.firstName))
            {
                user.FirstName = editProfileRequest.firstName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(editProfileRequest.lastName))
            {
                user.LastName = editProfileRequest.lastName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(editProfileRequest.email))
            {
                var email = EmailOrPhoneValidator.Normalize(editProfileRequest.email);
                if (EmailOrPhoneValidator.GetKind(email) != EmailOrPhoneKind.Email)
                {
                    return GeneralResponse.BadRequest("Messages.InvalidRequest");
                }

                if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase))
                {
                    var existUser = await _userManager.FindByEmailAsync(email);
                    if (existUser != null && existUser.Id != user.Id)
                    {
                        return GeneralResponse.BadRequest("Messages.EmailInUse");
                    }

                    user.Email = email;
                    user.EmailConfirmed = false;
                }
            }

            if (!string.IsNullOrWhiteSpace(editProfileRequest.phone))
            {
                var phone = EmailOrPhoneValidator.Normalize(editProfileRequest.phone);
                if (EmailOrPhoneValidator.GetKind(phone) != EmailOrPhoneKind.Phone)
                {
                    return GeneralResponse.BadRequest("Messages.InvalidRequest");
                }

                if (!string.Equals(user.PhoneNumber, phone, StringComparison.Ordinal))
                {
                    var existUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phone);
                    if (existUser != null && existUser.Id != user.Id)
                    {
                        return GeneralResponse.BadRequest("Messages.PhoneInUse");
                    }

                    user.PhoneNumber = phone;
                    user.PhoneNumberConfirmed = false;
                }
            }

            if (editProfileRequest.Picture != null)
            {
                if (!string.IsNullOrEmpty(user.ImagePath))
                {
                    var Deletedpath = user.ImagePath.Replace(_currentUserService.GetBaseUrl(""), "").Replace("\\\\", "\\");
                    await _fileService.DeleteFileAsync(Deletedpath);
                }
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(editProfileRequest.Picture.FileName)}";

                using var memoryStream = new MemoryStream();
                await editProfileRequest.Picture.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var savedPath = await _fileService.UploadFileAsync(FilePathType.UserProfiles, user.Id, fileName, fileBytes);

                user.ImagePath = savedPath;
            }

            try
            {
                await _userManager.UpdateAsync(user);
                if (!string.IsNullOrWhiteSpace(editProfileRequest.email) &&
                    !user.EmailConfirmed &&
                    !string.IsNullOrWhiteSpace(user.Email))
                {
                    await _verificationService.SendVerificationCodeAsync(user.Id, user.Email, "Email Verification", "ar");
                }
            }
            catch (Exception ex)
            {
                return GeneralResponse.InternalError("Messages.OperationFailed", new { Error = ex.Message });
            }
            return GeneralResponse.Ok("Messages.ProfileUpdated");
        }

        public async Task<GeneralResponse> AdminEditProfileAsync(AdminEditProfileRequest editProfileRequest)
        {
            var user = await _unitOfWork.Repository<Admin>().GetByIdAsync(editProfileRequest.Id);
            if (user == null)
                return GeneralResponse.NotFound("Messages.UserNotFound");

            user.FirstName = editProfileRequest.firstName.Trim();
            user.LastName = editProfileRequest.lastName.Trim();

            if (editProfileRequest.Picture != null)
            {
                if (!string.IsNullOrEmpty(user.ImagePath))
                {
                    var Deletedpath = user.ImagePath.Replace(_currentUserService.GetBaseUrl(""), "").Replace("\\\\", "\\");
                    await _fileService.DeleteFileAsync(Deletedpath);
                }
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(editProfileRequest.Picture.FileName)}";

                using var memoryStream = new MemoryStream();
                await editProfileRequest.Picture.CopyToAsync(memoryStream);
                var fileBytes = memoryStream.ToArray();

                var savedPath = await _fileService.UploadFileAsync(FilePathType.UserProfiles, user.Id, fileName, fileBytes);

                user.ImagePath = savedPath;
            }

            try
            {
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return GeneralResponse.InternalError("Messages.OperationFailed", new { Error = ex.Message });
            }
            return GeneralResponse.Ok("Messages.ProfileUpdated");
        }

        public async Task<GeneralResponse> GetMyProfileAsync()
        {
            var userId = _currentUserService.CurrentUserId;
            if (!userId.HasValue)
            {
                return GeneralResponse.Unauthorized("Messages.Unauthorized");
            }

            var userData = await _unitOfWork.Repository<ApplicationUser>().GetByIdAsync(userId.Value);
            if (userData == null)
                return GeneralResponse.NotFound("Messages.UserNotFound");
            
            var data = new GetUserByIdResponse()
            {
                CreatedOn = userData.CreatedOn,
                Email = userData.Email ?? string.Empty,
                EmailConfirmed = userData.EmailConfirmed,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                Id = userData.Id,
                IsActive = userData.IsActive,
                PhoneNumber = userData.PhoneNumber ?? string.Empty,
            };
            var imagePath = (await _fileService.GetFileUrlAsync(userData.ImagePath));
            data.ImagePath = imagePath != null ? imagePath : null;
            
            var userRoles = await _userManager.GetRolesAsync(userData);
            if (userRoles.Count > 0)
                data.Roles.AddRange(userRoles);
            
            return GeneralResponse.Ok("Messages.GetUserDataSuccess", data);
        }

    }
}
