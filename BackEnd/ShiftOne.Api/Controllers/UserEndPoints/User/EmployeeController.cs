using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Extensions;
using ShiftOne.Core.Interfaces.Application;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests.User;
using ShiftOne.Shared.Responses;
using ShiftOne.Shared.Responses.User;
using Swashbuckle.AspNetCore.Filters;

namespace ShiftOne.Api.Controllers.UserEndPoints.User
{
    [ApiController]
    [Route("api/employees/[controller]")]
    [ApiExplorerSettings(GroupName = "employees")]
    [ProducesResponseType(typeof(GeneralResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(void), StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(typeof(void), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(void), StatusCodes.Status401Unauthorized)]
    public class EmployeeController : ControllerBase
    {
        private readonly IUserService _userService;

        public EmployeeController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>Employee self-registration is disabled for the MVP. Employees are created from the dashboard.</summary>
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register()
        {
            return StatusCode(StatusCodes.Status410Gone, GeneralResponse.BadRequest("Messages.EmployeeSelfRegistrationDisabled"));
        }

        /// <summary>Refresh an employee access token using a valid refresh token.</summary>
        [AllowAnonymous]
        [HttpPost("RefreshToken")]
        [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.RefreshTokenAsync(request.RefreshToken);
            return result.ToActionResult();
        }

        /// <summary>Login an employee and bind the device on first employee login.</summary>
        [AllowAnonymous]
        [HttpPost("login")]
        [SwaggerRequestExample(typeof(LoginRequest), typeof(LoginRequest))]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.LoginAsync(loginRequest);
            return result.ToActionResult();
        }

        /// <summary>Send a password reset link to the employee email address.</summary>
        [AllowAnonymous]
        [HttpPost("SendPasswordResetUrl")]
        [SwaggerRequestExample(typeof(SendPasswordResetUrlRequest), typeof(SendPasswordResetUrlRequest))]
        public async Task<IActionResult> SendPasswordResetUrlAsync([FromBody] SendPasswordResetUrlRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.SendPasswordResetUrlAsync(request);
            return result.ToActionResult();
        }

        /// <summary>Reset an employee password using an email reset token.</summary>
        [AllowAnonymous]
        [HttpPost("ResetPassword")]
        [SwaggerRequestExample(typeof(ResetPasswordRequest), typeof(ResetPasswordRequest))]
        public async Task<IActionResult> ResetPasswordAsync([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.ResetPasswordAsync(resetPasswordRequest);
            return result.ToActionResult();
        }

        /// <summary>Reset an employee password using a phone verification code.</summary>
        [AllowAnonymous]
        [HttpPost("ResetPasswordByPhone")]
        [SwaggerRequestExample(typeof(ResetPasswordByPhoneRequest), typeof(ResetPasswordByPhoneRequest))]
        public async Task<IActionResult> ResetPasswordByPhoneAsync([FromBody] ResetPasswordByPhoneRequest resetPasswordRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.ResetPasswordByPhoneAsync(resetPasswordRequest);
            return result.ToActionResult();
        }

        /// <summary>Verify an employee email address using a verification code.</summary>
        [AllowAnonymous]
        [HttpPost("VerifyEmail")]
        [SwaggerRequestExample(typeof(VerifyEmailRequest), typeof(VerifyEmailRequest))]
        public async Task<IActionResult> VerifyEmailAsync([FromBody] VerifyEmailRequest verifyEmailRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.VerifyEmailAsync(verifyEmailRequest);
            return result.ToActionResult();
        }

        /// <summary>Verify an employee phone number using a verification code.</summary>
        [AllowAnonymous]
        [HttpPost("VerifyPhone")]
        [SwaggerRequestExample(typeof(VerifyPhoneRequest), typeof(VerifyPhoneRequest))]
        public async Task<IActionResult> VerifyPhoneAsync([FromBody] VerifyPhoneRequest verifyPhoneRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.VerifyPhoneAsync(verifyPhoneRequest);
            return result.ToActionResult();
        }

        /// <summary>Send an email verification code to an employee.</summary>
        [AllowAnonymous]
        [HttpPost("SendVerifyEmailCode")]
        [SwaggerRequestExample(typeof(SendVerifyEmailCodeRequest), typeof(SendVerifyEmailCodeRequest))]
        public async Task<IActionResult> SendVerifyEmailCodeAsync([FromBody] SendVerifyEmailCodeRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.SendVerifyEmailCodeAsync(request);
            return result.ToActionResult();
        }

        /// <summary>Update the authenticated employee profile.</summary>
        [HasPermission(Permissions.Profile.Edit)]
        [HttpPut("EditProfile")]
        [SwaggerRequestExample(typeof(EditProfileRequest), typeof(EditProfileRequest))]
        public async Task<IActionResult> EditProfileAsync([FromForm] EditProfileRequest editProfileRequest)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.EditProfileAsync(editProfileRequest);
            return result.ToActionResult();
        }

        /// <summary>Get the authenticated employee profile.</summary>
        [HasPermission(Permissions.Profile.View)]
        [HttpGet("getmyprofile")]
        [ProducesResponseType(typeof(GetUserByIdResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyProfileAsync()
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var result = await _userService.GetMyProfileAsync();
            return result.ToActionResult();
        }
    }
}