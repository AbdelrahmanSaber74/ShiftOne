using Microsoft.AspNetCore.Http;
using Moq;
using ShiftOne.Shared.Requests.User;
using ShiftOne.Shared.Responses.User;

namespace ShiftOne.Tests.Services.User;

public class UserServiceProfileTests
{
    [Fact]
    public async Task GetMyProfileAsync_WhenUserIsAnonymous_ReturnsUnauthorized()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.GetMyProfileAsync();

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
    }

    [Fact]
    public async Task GetMyProfileAsync_WhenCustomerMissing_ReturnsNotFound()
    {
        var context = new UserServiceTestContext();
        context.CurrentUserService.CurrentUserId = Guid.NewGuid();

        var response = await context.Service.GetMyProfileAsync();

        Assert.False(response.Success);
        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public async Task GetMyProfileAsync_WhenCustomerExists_ReturnsProfileWithRolesAndImageUrl()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: "user@test.com");
        user.ImagePath = "profiles/user.png";
        context.CurrentUserService.CurrentUserId = user.Id;

        var response = await context.Service.GetMyProfileAsync();

        Assert.True(response.Success);
        var data = Assert.IsType<GetUserByIdResponse>(response.Data);
        Assert.Equal(user.Id, data.Id);
        Assert.Equal("https://files.test/profiles/user.png", data.ImagePath);
        Assert.Contains("Customer", data.Roles);
    }

    [Fact]
    public async Task EditProfileAsync_WhenNoFieldsProvided_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();
        context.CurrentUserService.CurrentUserId = Guid.NewGuid();

        var response = await context.Service.EditProfileAsync(new EditProfileRequest());

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.InvalidProfileFields", response.Message);
    }

    [Fact]
    public async Task EditProfileAsync_WhenEmailBelongsToAnotherUser_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();
        var current = context.AddCustomer(email: "current@test.com");
        context.AddCustomer(email: "taken@test.com");
        context.CurrentUserService.CurrentUserId = current.Id;

        var response = await context.Service.EditProfileAsync(new EditProfileRequest
        {
            email = "taken@test.com"
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.EmailInUse", response.Message);
    }

    [Fact]
    public async Task EditProfileAsync_WhenPhoneBelongsToAnotherUser_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();
        var current = context.AddCustomer(email: "current@test.com", phone: "+201000000000");
        context.AddCustomer(email: "other@test.com", phone: "+202000000000");
        context.CurrentUserService.CurrentUserId = current.Id;

        var response = await context.Service.EditProfileAsync(new EditProfileRequest
        {
            phone = "+202000000000"
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.PhoneInUse", response.Message);
    }

    [Fact]
    public async Task EditProfileAsync_WhenEmailChanges_UpdatesUserAndSendsVerificationCode()
    {
        var context = new UserServiceTestContext();
        var current = context.AddCustomer(email: "current@test.com", emailConfirmed: true);
        context.CurrentUserService.CurrentUserId = current.Id;

        var response = await context.Service.EditProfileAsync(new EditProfileRequest
        {
            firstName = " Updated ",
            lastName = " Name ",
            email = "new@test.com"
        });

        Assert.True(response.Success);
        Assert.Equal("Updated", current.FirstName);
        Assert.Equal("Name", current.LastName);
        Assert.Equal("new@test.com", current.Email);
        Assert.False(current.EmailConfirmed);
        context.UserManager.Verify(manager => manager.UpdateAsync(current), Times.Once);
        context.VerificationService.Verify(
            service => service.SendVerificationCodeAsync(current.Id, "new@test.com", "Email Verification", "ar"),
            Times.Once);
    }

    [Fact]
    public async Task EditProfileAsync_WhenPictureChanges_DeletesOldImageAndUploadsNewImage()
    {
        var context = new UserServiceTestContext();
        var current = context.AddCustomer(email: "current@test.com");
        current.ImagePath = "https://api.test/old/path.png";
        context.CurrentUserService.CurrentUserId = current.Id;

        var response = await context.Service.EditProfileAsync(new EditProfileRequest
        {
            Picture = CreateFormFile("avatar.png", "image-data")
        });

        Assert.True(response.Success);
        Assert.StartsWith($"uploads/{current.Id}/", current.ImagePath);
        context.FileService.Verify(service => service.DeleteFileAsync("old/path.png"), Times.Once);
        context.FileService.Verify(
            service => service.UploadFileAsync(
                ShiftOne.Shared.Constants.FilePathType.UserProfiles,
                current.Id,
                It.Is<string>(name => name.EndsWith(".png")),
                It.IsAny<byte[]>()),
            Times.Once);
    }

    [Fact]
    public async Task AdminEditProfileAsync_WhenAdminMissing_ReturnsNotFound()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.AdminEditProfileAsync(new AdminEditProfileRequest
        {
            Id = Guid.NewGuid(),
            firstName = "Admin",
            lastName = "User"
        });

        Assert.False(response.Success);
        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public async Task AdminEditProfileAsync_WhenAdminExists_UpdatesProfile()
    {
        var context = new UserServiceTestContext();
        var admin = context.AddAdmin();

        var response = await context.Service.AdminEditProfileAsync(new AdminEditProfileRequest
        {
            Id = admin.Id,
            firstName = "Root",
            lastName = "Admin"
        });

        Assert.True(response.Success);
        Assert.Equal("Root", admin.FirstName);
        Assert.Equal("Admin", admin.LastName);
        context.UserManager.Verify(manager => manager.UpdateAsync(admin), Times.Once);
    }

    private static IFormFile CreateFormFile(string fileName, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName);
    }
}
