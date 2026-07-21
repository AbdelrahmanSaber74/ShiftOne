using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Moq;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Shared.Requests.User;
using ShiftOne.Shared.Responses.User;

namespace ShiftOne.Tests.Services.User;

public class UserServiceAuthenticationTests
{
    [Fact]
    public async Task RegisterAsync_WhenEmailAlreadyExists_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();
        context.AddCustomer(email: "user@test.com");

        var response = await context.Service.RegisterAsync(new RegisterRequest
        {
            firstName = "New",
            lastName = "User",
            emailOrPhone = "USER@test.com",
            password = "Password1"
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.EmailInUse", response.Message);
    }

    [Fact]
    public async Task RegisterAsync_WhenPhoneAlreadyExists_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();
        context.AddCustomer(email: null, phone: "+201000000000");

        var response = await context.Service.RegisterAsync(new RegisterRequest
        {
            firstName = "New",
            lastName = "User",
            emailOrPhone = "+201000000000",
            password = "Password1"
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.PhoneInUse", response.Message);
    }

    [Fact]
    public async Task RegisterAsync_WhenValidEmail_CreatesActiveCustomerAndAddsRole()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.RegisterAsync(new RegisterRequest
        {
            firstName = "  New  ",
            lastName = "  User  ",
            emailOrPhone = "new@test.com",
            password = "Password1"
        });

        Assert.True(response.Success);
        var created = Assert.Single(context.Customers);
        Assert.Equal("New", created.FirstName);
        Assert.Equal("User", created.LastName);
        Assert.Equal("new@test.com", created.Email);
        Assert.True(created.IsActive);
        Assert.Contains("Customer", context.UserRoles[created.Id]);
    }

    [Fact]
    public async Task RegisterAsync_WhenPictureProvided_StoresImagePath()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.RegisterAsync(new RegisterRequest
        {
            firstName = "New",
            lastName = "User",
            emailOrPhone = "new@test.com",
            password = "Password1",
            Picture = CreateFormFile("avatar.png", "image-data")
        });

        Assert.True(response.Success);
        var created = Assert.Single(context.Customers);
        Assert.StartsWith($"/uploads/UserProfiles/{created.Id}/", created.ImagePath);
        context.FileService.Verify(
            service => service.UploadImageAsync(ShiftOne.Shared.Constants.FilePathType.UserProfiles, created.Id, It.IsAny<IFormFile>()),
            Times.Once);
        context.UserManager.Verify(manager => manager.UpdateAsync(created), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WhenPictureUploadFails_ReturnsBadRequestWithoutImagePath()
    {
        var context = new UserServiceTestContext();
        context.FileService.Setup(service => service.UploadImageAsync(It.IsAny<ShiftOne.Shared.Constants.FilePathType>(), It.IsAny<Guid>(), It.IsAny<IFormFile>()))
            .ThrowsAsync(new InvalidOperationException("Only jpg, jpeg, png, and webp images are allowed."));

        var response = await context.Service.RegisterAsync(new RegisterRequest
        {
            firstName = "New",
            lastName = "User",
            emailOrPhone = "new@test.com",
            password = "Password1",
            Picture = CreateFormFile("avatar.exe", "not-image")
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.OperationFailed", response.Message);
        var created = Assert.Single(context.Customers);
        Assert.Null(created.ImagePath);
    }

    [Fact]
    public async Task LoginAsync_WhenUserMissing_ReturnsGenericInvalidCredentials()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.LoginAsync(new LoginRequest
        {
            emailOrPhone = "missing@test.com",
            password = "Password1"
        });

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
        Assert.Equal("Messages.InvalidEmailPhoneOrPassword", response.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenPasswordWrong_ReturnsGenericInvalidCredentialsAndAccessFails()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: "user@test.com", password: "Password1");

        var response = await context.Service.LoginAsync(new LoginRequest
        {
            emailOrPhone = "user@test.com",
            password = "WrongPass1"
        });

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
        Assert.Equal("Messages.InvalidEmailPhoneOrPassword", response.Message);
        context.UserManager.Verify(manager => manager.AccessFailedAsync(user), Times.Once);
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    public async Task LoginAsync_WhenUserNotAllowed_ReturnsGenericInvalidCredentials(
        bool isActive,
        bool unlocked)
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(
            email: "user@test.com",
            isActive: isActive,
            emailConfirmed: true,
            password: "Password1");
        if (!unlocked)
        {
            context.LockedUsers.Add(user.Id);
        }

        var response = await context.Service.LoginAsync(new LoginRequest
        {
            emailOrPhone = "user@test.com",
            password = "Password1"
        });

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
        Assert.Equal("Messages.InvalidEmailPhoneOrPassword", response.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenEmailNotConfirmedAndPasswordCorrect_AsksUserToVerifyAccount()
    {
        var context = new UserServiceTestContext();
        context.AddCustomer(
            email: "user@test.com",
            isActive: true,
            emailConfirmed: false,
            password: "Password1");

        var response = await context.Service.LoginAsync(new LoginRequest
        {
            emailOrPhone = "user@test.com",
            password = "Password1"
        });

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
        Assert.Equal("Messages.VerifyAccount", response.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenPhoneNotConfirmedAndPasswordCorrect_AsksUserToVerifyAccount()
    {
        var context = new UserServiceTestContext();
        context.AddCustomer(
            email: null,
            phone: "+201000000000",
            isActive: true,
            phoneConfirmed: false,
            password: "Password1");

        var response = await context.Service.LoginAsync(new LoginRequest
        {
            emailOrPhone = "+201000000000",
            password = "Password1"
        });

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
        Assert.Equal("Messages.VerifyAccount", response.Message);
    }

    [Fact]
    public async Task LoginAsync_WhenEmailNotConfirmedAndPasswordWrong_ReturnsGenericInvalidCredentials()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(
            email: "user@test.com",
            isActive: true,
            emailConfirmed: false,
            password: "Password1");

        var response = await context.Service.LoginAsync(new LoginRequest
        {
            emailOrPhone = "user@test.com",
            password = "WrongPass1"
        });

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
        Assert.Equal("Messages.InvalidEmailPhoneOrPassword", response.Message);
        context.UserManager.Verify(manager => manager.AccessFailedAsync(user), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_WhenValidCustomer_ReturnsAccessAndRefreshTokens()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: "user@test.com", password: "Password1");

        var response = await context.Service.LoginAsync(new LoginRequest
        {
            emailOrPhone = "user@test.com",
            password = "Password1"
        });

        Assert.True(response.Success);
        var data = Assert.IsType<LoginResponse>(response.Data);
        Assert.Equal($"access:{user.Id}", data.AccessToken);
        Assert.Equal("refresh-1", data.RefreshToken);
        Assert.Contains(context.RefreshTokens, token => token.ApplicationUserId == user.Id && token.TokenHash == "hash:refresh-1");
        context.UserManager.Verify(manager => manager.ResetAccessFailedCountAsync(user), Times.Once);
    }

    [Fact]
    public async Task AdminLoginAsync_WhenRoleIsInactive_ReturnsGenericInvalidCredentials()
    {
        var context = new UserServiceTestContext();
        context.AddAdmin(email: "admin@test.com", password: "Password1");

        var response = await context.Service.AdminLoginAsync(new LoginRequest
        {
            emailOrPhone = "admin@test.com",
            password = "Password1"
        });

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
        Assert.Equal("Messages.InvalidEmailPhoneOrPassword", response.Message);
    }

    [Fact]
    public async Task AdminLoginAsync_WhenAdminRoleIsActive_ReturnsTokens()
    {
        var context = new UserServiceTestContext();
        context.AddActiveRole("Admin");
        var admin = context.AddAdmin(email: "admin@test.com", password: "Password1");

        var response = await context.Service.AdminLoginAsync(new LoginRequest
        {
            emailOrPhone = "admin@test.com",
            password = "Password1"
        });

        Assert.True(response.Success);
        var data = Assert.IsType<LoginResponse>(response.Data);
        Assert.Equal($"access:{admin.Id}", data.AccessToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenMissing_ReturnsUnauthorized()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.RefreshTokenAsync("missing-refresh");

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
        Assert.Equal("Messages.Unauthorized", response.Message);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenTokenReused_RevokesActiveTokensForUser()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer();
        context.AddRefreshToken(user, "old", active: false);
        var active = context.AddRefreshToken(user, "active", active: true);

        var response = await context.Service.RefreshTokenAsync("old");

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
        Assert.True(active.IsRevoked);
        Assert.Equal("Refresh token reuse detected.", active.ReasonRevoked);
    }

    [Fact]
    public async Task RefreshTokenAsync_WhenValid_RotatesRefreshToken()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer();
        var savedToken = context.AddRefreshToken(user, "old", active: true);

        var response = await context.Service.RefreshTokenAsync("old");

        Assert.True(response.Success);
        var data = Assert.IsType<RefreshTokenResponse>(response.Data);
        Assert.Equal("refresh-1", data.RefreshToken);
        Assert.True(savedToken.IsRevoked);
        Assert.Equal("hash:refresh-1", savedToken.ReplacedByTokenHash);
        Assert.Contains(context.RefreshTokens, token => token.TokenHash == "hash:refresh-1" && token.ApplicationUserId == user.Id);
    }

    [Fact]
    public async Task AdminRefreshTokenAsync_WhenUserIsNotAdmin_ReturnsUnauthorized()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer();
        context.AddRefreshToken(user, "old", active: true);

        var response = await context.Service.AdminRefreshTokenAsync("old");

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
        Assert.Equal("Messages.Unauthorized", response.Message);
    }

    [Fact]
    public async Task RevokeTokenAsync_WhenTokenExists_RevokesToken()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer();
        var token = context.AddRefreshToken(user, "refresh-token", active: true);

        var response = await context.Service.RevokeTokenAsync("refresh-token");

        Assert.True(response.Success);
        Assert.True(token.IsRevoked);
        Assert.Equal("Revoked by user.", token.ReasonRevoked);
    }

    private static IFormFile CreateFormFile(string fileName, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName);
    }
}
