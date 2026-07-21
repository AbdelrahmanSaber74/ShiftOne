using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.User;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.User;
using Swashbuckle.AspNetCore.Filters;
using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Api.Controllers.DashboardEndPoints.Admin
{
    [Route("api/dashboard/[controller]")]
    [ApiExplorerSettings(GroupName = "dashboard")]
    [ApiController]
    [ProducesResponseType(typeof(GeneralResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public class AdminController : ControllerBase
    {
        private readonly IUserService _userService;

        public AdminController(IUserService userService)
        {
            _userService = userService;
        }
        /// <summary>
        /// Get current authenticated admin user with roles and permissions
        /// </summary>
        /// <returns></returns>
        [HasPermission(Permissions.Profile.View)]
        [HttpGet("getcurrentuserinfo")]
        [ProducesResponseType(typeof(AdminCurrentUserResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrentUserInfoAsync()
        {
            var result = await _userService.GetCurrentAdminContextAsync();
            return result.ToActionResult();
        }
        /// <summary>
        /// Refresh Token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.AdminRefreshTokenAsync(request.RefreshToken);
            return result.ToActionResult();
        }
        /// <summary>
        /// Revoke Token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HasPermission(Permissions.Users.Delete)]
        [HttpPut("RevokToken")]
        public async Task<IActionResult> RevokeTokenAsync([FromBody] RevokeTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.RevokeTokenAsync(request.RefreshToken);
            return result.ToActionResult();
        }
        /// <summary>
        /// Login
        /// </summary>
        /// <param name="loginRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("login")]
        [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequest))]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.AdminLoginAsync(loginRequest);
            return result.ToActionResult();
        }
        /// <summary>
        /// Send Password Reset Url
        /// </summary>
        [AllowAnonymous]
        [HttpPost("SendPasswordResetUrl")]
        [SwaggerRequestExample(typeof(SendPasswordResetUrlRequest), typeof(SendPasswordResetUrlRequest))]
        public async Task<IActionResult> SendPasswordResetUrlAsync([FromBody] SendPasswordResetUrlRequest sendPasswordResetCodeReques)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.SendPasswordResetUrlAsync(sendPasswordResetCodeReques);
            return result.ToActionResult();
        }        
        /// <summary>
        /// Reset Password
        /// </summary>
        /// <param name="resetPasswordRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        [SwaggerRequestExample(typeof(ResetPasswordRequest), typeof(ResetPasswordRequest))]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.ResetPasswordAsync(resetPasswordRequest);
            return result.ToActionResult();
        }
        /// <summary>
        /// Reset Password By Phone
        /// </summary>
        /// <param name="resetPasswordRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("ResetPasswordByPhone")]
        [SwaggerRequestExample(typeof(ResetPasswordByPhoneRequest), typeof(ResetPasswordByPhoneRequest))]
        public async Task<IActionResult> ResetPasswordByPhoneAsync([FromBody] ResetPasswordByPhoneRequest resetPasswordRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.ResetPasswordByPhoneAsync(resetPasswordRequest);
            return result.ToActionResult();
        }
        /// <summary>
        /// Verify Email
        /// </summary>
        /// <param name="verifyEmailRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("VerifyEmail")]
        [SwaggerRequestExample(typeof(VerifyEmailRequest), typeof(VerifyEmailRequest))]
        public async Task<IActionResult> VerifyEmailAsync([FromBody] VerifyEmailRequest verifyEmailRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.VerifyEmailAsync(verifyEmailRequest);
            return result.ToActionResult();
        }
        /// <summary>
        /// Verify Phone
        /// </summary>
        /// <param name="verifyPhoneRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("VerifyPhone")]
        [SwaggerRequestExample(typeof(VerifyPhoneRequest), typeof(VerifyPhoneRequest))]
        public async Task<IActionResult> VerifyPhoneAsync([FromBody] VerifyPhoneRequest verifyPhoneRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.VerifyPhoneAsync(verifyPhoneRequest);
            return result.ToActionResult();
        }
        /// <summary>
        /// Send Verify Email Code
        /// </summary>
        /// <param name="sendVerifyEmailCodeRequest"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost("SendVerifyEmailCode")]
        [SwaggerRequestExample(typeof(SendVerifyEmailCodeRequest), typeof(SendVerifyEmailCodeRequest))]
        public async Task<IActionResult> SendVerifyEmailCodeAsync([FromBody] SendVerifyEmailCodeRequest sendVerifyEmailCodeRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.SendVerifyEmailCodeAsync(sendVerifyEmailCodeRequest);
            return result.ToActionResult();
        }        
        /// <summary>
        /// Approve User
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HasPermission(Permissions.Users.Approve)]
        [HttpPut("approve")]
        public async Task<IActionResult> ApproveUser([FromQuery][Required] Guid userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.ApproveUserAsync(userId);
            return result.ToActionResult();
        }
        /// <summary>
        /// Edit Profile
        /// </summary>
        /// <param name="editProfileRequest"></param>
        /// <returns></returns>
        [HasPermission(Permissions.Users.Edit)]
        [HttpPut("EditProfile")]
        public async Task<IActionResult> EditProfileAsync([FromForm] AdminEditProfileRequest editProfileRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.AdminEditProfileAsync(editProfileRequest);
            return result.ToActionResult();
        }
        /// <summary>
        /// Force reset user password (admin).
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HasPermission(Permissions.Users.Edit)]
        [HttpPatch("reset-password")]
        public async Task<IActionResult> ResetUserPasswordAsync([FromBody] AdminResetUserPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.AdminResetUserPasswordAsync(request);
            return result.ToActionResult();
        }
        /// <summary>
        /// Force verify user email (admin).
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HasPermission(Permissions.Users.Edit)]
        [HttpPatch("activate-email")]
        public async Task<IActionResult> ActivateUserEmailAsync([FromBody] AdminActivateUserEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.AdminActivateUserEmailAsync(request);
            return result.ToActionResult();
        }
        /// <summary>
        /// UnApprove User
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HasPermission(Permissions.Users.Approve)]
        [HttpPut("unapprove")]
        public async Task<IActionResult> UnApproveUserAsync([FromQuery][Required] Guid userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.UnApproveUserAsync(userId);
            return result.ToActionResult();
        }                
        /// <summary>
        /// Get User By Id
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        [HasPermission(Permissions.Users.View)]
        [HttpGet("GetUserById")]
        [ProducesResponseType(typeof(GetUserByIdResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserByIdAsync([FromQuery][Required] Guid userId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.AdminGetUserByIdAsync(userId);
            return result.ToActionResult();
        }
        /// <summary>
        /// Get All Users
        /// </summary>
        /// <param name="paginationRequest"></param>
        /// <param name="keyword"></param>
        /// <param name="isActive"></param>
        /// <returns></returns>
        [HasPermission(Permissions.Users.View)]
        [HttpGet("GetAllUsers")]
        [ProducesResponseType(typeof(GetAllUsersResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsersAsync([FromQuery] PaginationRequest paginationRequest,[FromQuery] string? keyword,[FromQuery] bool? isActive)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var result = await _userService.GetAllUsersAsync(paginationRequest,keyword,isActive);
            return result.ToActionResult();
        }
    }
}




