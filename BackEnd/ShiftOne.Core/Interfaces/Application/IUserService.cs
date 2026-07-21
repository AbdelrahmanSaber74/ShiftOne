using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.User;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Core.Interfaces.Application
{
    public interface IUserService
    {
        Task<GeneralResponse> RegisterAsync(RegisterRequest registerRequest);
        Task<GeneralResponse> RevokeTokenAsync(string refreshToken);
        Task<GeneralResponse> LoginAsync(LoginRequest loginRequest);
        Task<GeneralResponse> AdminLoginAsync(LoginRequest loginRequest);
        Task<GeneralResponse> RefreshTokenAsync(string refreshToken);
        Task<GeneralResponse> AdminRefreshTokenAsync(string refreshToken);
        Task<GeneralResponse> GetAllUsersAsync(PaginationRequest paginationRequest, string? keyword, bool? isActive);
        Task<GeneralResponse> GetMyProfileAsync();
        Task<GeneralResponse> GetCurrentAdminContextAsync();
        Task<GeneralResponse> AdminGetUserByIdAsync(Guid userId);
        Task<GeneralResponse> SendVerifyEmailCodeAsync(SendVerifyEmailCodeRequest sendVerifyEmailCodeRequest);
        Task<GeneralResponse> VerifyEmailAsync(VerifyEmailRequest verifyEmailRequest);
        Task<GeneralResponse> VerifyPhoneAsync(VerifyPhoneRequest verifyEmailRequest);
        Task<GeneralResponse> SendPasswordResetUrlAsync(SendPasswordResetUrlRequest sendPasswordResetUrlRequest);
        Task<GeneralResponse> ResetPasswordAsync(ResetPasswordRequest resetPasswordRequest);
        Task<GeneralResponse> ResetPasswordByPhoneAsync(ResetPasswordByPhoneRequest resetPasswordRequest);
        Task<GeneralResponse> ApproveUserAsync(Guid userId);
        Task<GeneralResponse> UnApproveUserAsync(Guid userId);
        Task<GeneralResponse> AdminResetUserPasswordAsync(AdminResetUserPasswordRequest request);
        Task<GeneralResponse> AdminActivateUserEmailAsync(AdminActivateUserEmailRequest request);
        Task<GeneralResponse> EditProfileAsync(EditProfileRequest editProfileRequest);
        Task<GeneralResponse> AdminEditProfileAsync(AdminEditProfileRequest editProfileRequest);
    }
}


