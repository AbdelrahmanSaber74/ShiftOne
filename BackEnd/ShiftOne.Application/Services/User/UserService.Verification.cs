using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Identity;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Shared.Requests.User;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Application.Services.User
{
    public partial class UserService
    {
        public async Task<GeneralResponse> SendVerifyEmailCodeAsync(SendVerifyEmailCodeRequest sendVerifyEmailCodeRequest)
        {
            var user = await _userManager.FindByEmailAsync(sendVerifyEmailCodeRequest.email);
            if (user == null)
                return GeneralResponse.Ok("Messages.EmailVerificationCodeSent");

            try
            {
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    return GeneralResponse.Ok("Messages.EmailVerificationCodeSent");
                }

                await _verificationService.SendVerificationCodeAsync(user.Id, user.Email, "Messages.VerifyEmailCode", "ar");
            }
            catch (Exception ex)
            {
                return GeneralResponse.InternalError("Messages.OperationFailed", new { Error = ex.Message });
            }

            return GeneralResponse.Ok("Messages.EmailVerificationCodeSent");
        }

        public async Task<GeneralResponse> SendVerifyPhoneCodeAsync(SendVerifyPhoneCodeRequest sendVerifyPhoneCodeRequest)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == sendVerifyPhoneCodeRequest.phone);
            if (user == null)
                return GeneralResponse.Ok("Messages.Success");

            try
            {
                if (string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    return GeneralResponse.Ok("Messages.Success");
                }

                await _verificationService.GenerateAndSavePhoneCodeAsync(user.Id, user.PhoneNumber);
            }
            catch (Exception ex)
            {
                return GeneralResponse.InternalError("Messages.OperationFailed", new { Error = ex.Message });
            }

            return GeneralResponse.Ok("Messages.Success");
        }

        public async Task<GeneralResponse> VerifyEmailAsync(VerifyEmailRequest verifyEmailRequest)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(verifyEmailRequest.email);
                if (user == null)
                    return GeneralResponse.BadRequest("Messages.InvalidRequest");
                if (!await _verificationService.VerifyCodeAsync(user.Id, verifyEmailRequest.verificationCode))
                    return GeneralResponse.BadRequest("Messages.CodeExpiredOrInvalid");
                user.EmailConfirmed = true;
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return GeneralResponse.InternalError("Messages.OperationFailed", new { Error = ex.Message });
            }

            return GeneralResponse.Ok("Messages.EmailVerified");
        }

        public async Task<GeneralResponse> VerifyPhoneAsync(VerifyPhoneRequest verifyEmailRequest)
        {
            try
            {
                var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == verifyEmailRequest.phone);
                if (user == null)
                    return GeneralResponse.BadRequest("Messages.InvalidRequest");
                if (!await _verificationService.VerifyCodeAsync(user.Id, verifyEmailRequest.verificationCode))
                    return GeneralResponse.BadRequest("Messages.CodeExpiredOrInvalid");
                user.PhoneNumberConfirmed = true;
                await _userManager.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                return GeneralResponse.InternalError("Messages.OperationFailed", new { Error = ex.Message });
            }

            return GeneralResponse.Ok("Messages.Success");
        }

        public async Task<GeneralResponse> SendPasswordResetUrlAsync(SendPasswordResetUrlRequest sendPasswordResetUrlRequest)
        {
            var user = await _userManager.FindByEmailAsync(sendPasswordResetUrlRequest.email);
            if (user == null)
                return GeneralResponse.Ok("Messages.Success");

            try
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                if (string.IsNullOrWhiteSpace(user.Email))
                {
                    return GeneralResponse.Ok("Messages.Success");
                }

                await _verificationService.SendRestpageUrlAsync(user.Email, "Messages.VerifyEmailCode", resetToken, sendPasswordResetUrlRequest.restLink, "ar");
            }
            catch (Exception ex)
            {
                return GeneralResponse.InternalError("Messages.OperationFailed", new { Error = ex.Message });
            }

            return GeneralResponse.Ok("Messages.Success");
        }

        public async Task<GeneralResponse> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest)
        {
            var user = await _userManager.FindByEmailAsync(resetPasswordRequest.email);
            if (user == null)
                return GeneralResponse.BadRequest("Messages.InvalidRequest");
            resetPasswordRequest.resetToken = resetPasswordRequest.resetToken.Replace("%2F", "/").Replace("%2B", "+");
            try
            {
                var res = await _userManager.ResetPasswordAsync(user, resetPasswordRequest.resetToken, resetPasswordRequest.newPassword);
                if (!res.Succeeded)
                {
                    return GeneralResponse.BadRequest("Messages.InvalidRequest");
                }

                await RevokeActiveRefreshTokensAsync(user.Id, "Password reset.");
            }
            catch (Exception ex)
            {
                return GeneralResponse.InternalError("Messages.OperationFailed", new { Error = ex.Message });
            }
            return GeneralResponse.Ok("Messages.PasswordChanged");
        }

        public async Task<GeneralResponse> ResetPasswordByPhoneAsync(ResetPasswordByPhoneRequest resetPasswordRequest)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == resetPasswordRequest.phone);
            if (user == null)
                return GeneralResponse.BadRequest("Messages.InvalidRequest");
            var isCodeValid = await _verificationService.VerifyCodeAsync(user.Id, resetPasswordRequest.Code);
            if (!isCodeValid)
                return GeneralResponse.BadRequest("Messages.InvalidRequest");
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            try
            {
                var res = await _userManager.ResetPasswordAsync(user, resetToken, resetPasswordRequest.newPassword);
                if (!res.Succeeded)
                {
                    return GeneralResponse.BadRequest("Messages.InvalidRequest");
                }

                await RevokeActiveRefreshTokensAsync(user.Id, "Password reset.");
            }
            catch (Exception ex)
            {
                return GeneralResponse.InternalError("Messages.OperationFailed", new { Error = ex.Message });
            }
            return GeneralResponse.Ok("Messages.PasswordChanged");
        }
    }
}
