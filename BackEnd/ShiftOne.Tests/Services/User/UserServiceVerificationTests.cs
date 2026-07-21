using Microsoft.AspNetCore.Identity;
using Moq;
using ShiftOne.Shared.Requests.User;

namespace ShiftOne.Tests.Services.User;

public class UserServiceVerificationTests
{
    [Fact]
    public async Task SendVerifyEmailCodeAsync_WhenUserMissing_ReturnsGenericSuccessAndDoesNotSend()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.SendVerifyEmailCodeAsync(new SendVerifyEmailCodeRequest
        {
            email = "missing@test.com"
        });

        Assert.True(response.Success);
        Assert.Equal("Messages.EmailVerificationCodeSent", response.Message);
        context.VerificationService.Verify(
            service => service.SendVerificationCodeAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task SendVerifyEmailCodeAsync_WhenUserExists_SendsCodeAndReturnsGenericSuccess()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: "user@test.com");

        var response = await context.Service.SendVerifyEmailCodeAsync(new SendVerifyEmailCodeRequest
        {
            email = "user@test.com"
        });

        Assert.True(response.Success);
        context.VerificationService.Verify(
            service => service.SendVerificationCodeAsync(user.Id, "user@test.com", "Messages.VerifyEmailCode", "ar"),
            Times.Once);
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenUserMissing_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.VerifyEmailAsync(new VerifyEmailRequest
        {
            email = "missing@test.com",
            verificationCode = "123456"
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.InvalidRequest", response.Message);
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenCodeInvalid_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();
        context.AddCustomer(email: "user@test.com", emailConfirmed: false);

        var response = await context.Service.VerifyEmailAsync(new VerifyEmailRequest
        {
            email = "user@test.com",
            verificationCode = "000000"
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.CodeExpiredOrInvalid", response.Message);
    }

    [Fact]
    public async Task VerifyEmailAsync_WhenCodeValid_ConfirmsEmail()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: "user@test.com", emailConfirmed: false);
        context.VerificationService.Setup(service => service.VerifyCodeAsync(user.Id, "123456"))
            .ReturnsAsync(true);

        var response = await context.Service.VerifyEmailAsync(new VerifyEmailRequest
        {
            email = "user@test.com",
            verificationCode = "123456"
        });

        Assert.True(response.Success);
        Assert.True(user.EmailConfirmed);
        context.UserManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task VerifyPhoneAsync_WhenCodeValid_ConfirmsPhone()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: null, phone: "+201000000000", phoneConfirmed: false);
        context.VerificationService.Setup(service => service.VerifyCodeAsync(user.Id, "123456"))
            .ReturnsAsync(true);

        var response = await context.Service.VerifyPhoneAsync(new VerifyPhoneRequest
        {
            phone = "+201000000000",
            verificationCode = "123456"
        });

        Assert.True(response.Success);
        Assert.True(user.PhoneNumberConfirmed);
        context.UserManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task SendPasswordResetUrlAsync_WhenUserMissing_ReturnsGenericSuccess()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.SendPasswordResetUrlAsync(new SendPasswordResetUrlRequest
        {
            email = "missing@test.com",
            restLink = "https://app/reset"
        });

        Assert.True(response.Success);
        Assert.Equal("Messages.Success", response.Message);
    }

    [Fact]
    public async Task SendPasswordResetUrlAsync_WhenUserExists_SendsResetUrl()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: "user@test.com");
        context.NextResetToken = "token-1";

        var response = await context.Service.SendPasswordResetUrlAsync(new SendPasswordResetUrlRequest
        {
            email = "user@test.com",
            restLink = "https://app/reset"
        });

        Assert.True(response.Success);
        context.VerificationService.Verify(
            service => service.SendRestpageUrlAsync("user@test.com", "Messages.VerifyEmailCode", "token-1", "https://app/reset", "ar"),
            Times.Once);
        context.UserManager.Verify(manager => manager.GeneratePasswordResetTokenAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenUserMissing_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.ResetPasswordAsync(new ResetPasswordRequest
        {
            email = "missing@test.com",
            resetToken = "token",
            newPassword = "NewPass1"
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.InvalidRequest", response.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenResetFails_ReturnsGenericBadRequest()
    {
        var context = new UserServiceTestContext();
        context.AddCustomer(email: "user@test.com");
        context.ResetPasswordResult = IdentityResult.Failed(new IdentityError { Description = "bad token" });

        var response = await context.Service.ResetPasswordAsync(new ResetPasswordRequest
        {
            email = "user@test.com",
            resetToken = "bad-token",
            newPassword = "NewPass1"
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.InvalidRequest", response.Message);
    }

    [Fact]
    public async Task ResetPasswordAsync_WhenSuccessful_RevokesActiveRefreshTokens()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: "user@test.com");
        var token = context.AddRefreshToken(user, "old", active: true);

        var response = await context.Service.ResetPasswordAsync(new ResetPasswordRequest
        {
            email = "user@test.com",
            resetToken = "token%2Fwith%2Bencoding",
            newPassword = "NewPass1"
        });

        Assert.True(response.Success);
        Assert.True(token.IsRevoked);
        Assert.Equal("Password reset.", token.ReasonRevoked);
        Assert.Equal("NewPass1", context.Passwords[user.Id]);
    }

    [Fact]
    public async Task ResetPasswordByPhoneAsync_WhenCodeInvalid_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();
        context.AddCustomer(email: null, phone: "+201000000000");

        var response = await context.Service.ResetPasswordByPhoneAsync(new ResetPasswordByPhoneRequest
        {
            phone = "+201000000000",
            Code = "000000",
            newPassword = "NewPass1"
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.InvalidRequest", response.Message);
    }

    [Fact]
    public async Task ResetPasswordByPhoneAsync_WhenSuccessful_ResetsPasswordAndRevokesTokens()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: null, phone: "+201000000000");
        var token = context.AddRefreshToken(user, "old", active: true);
        context.VerificationService.Setup(service => service.VerifyCodeAsync(user.Id, "123456"))
            .ReturnsAsync(true);

        var response = await context.Service.ResetPasswordByPhoneAsync(new ResetPasswordByPhoneRequest
        {
            phone = "+201000000000",
            Code = "123456",
            newPassword = "NewPass1"
        });

        Assert.True(response.Success);
        Assert.True(token.IsRevoked);
        Assert.Equal("NewPass1", context.Passwords[user.Id]);
    }
}
