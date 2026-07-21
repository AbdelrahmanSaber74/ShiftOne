using Microsoft.AspNetCore.Identity;
using Moq;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests;
using ShiftOne.Shared.Requests.User;
using ShiftOne.Shared.Responses.User;

namespace ShiftOne.Tests.Services.User;

public class UserServiceAdminTests
{
    [Fact]
    public async Task ApproveUserAsync_WhenUserMissing_ReturnsNotFound()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.ApproveUserAsync(Guid.NewGuid());

        Assert.False(response.Success);
        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public async Task ApproveUserAsync_WhenAlreadyActive_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(isActive: true);

        var response = await context.Service.ApproveUserAsync(user.Id);

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.UserAlreadyApproved", response.Message);
    }

    [Fact]
    public async Task ApproveUserAsync_WhenUpdateFails_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(isActive: false);
        context.UpdateResult = IdentityResult.Failed(new IdentityError { Description = "failed" });

        var response = await context.Service.ApproveUserAsync(user.Id);

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task ApproveUserAsync_WhenValid_ActivatesUser()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(isActive: false);

        var response = await context.Service.ApproveUserAsync(user.Id);

        Assert.True(response.Success);
        Assert.True(user.IsActive);
        context.UserManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task UnApproveUserAsync_WhenValid_DeactivatesUser()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(isActive: true);

        var response = await context.Service.UnApproveUserAsync(user.Id);

        Assert.True(response.Success);
        Assert.False(user.IsActive);
        context.UserManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task GetAllUsersAsync_ReturnsPagedUsersAndSkipsAdmins()
    {
        var context = new UserServiceTestContext();
        var first = context.AddCustomer(email: "first@test.com");
        first.CreatedOn = DateTime.UtcNow.AddDays(-1);
        var second = context.AddCustomer(email: "second@test.com");
        second.CreatedOn = DateTime.UtcNow;
        var adminCustomer = context.AddCustomer(email: "admin-customer@test.com");
        context.AddRole(adminCustomer, "Admin");

        var response = await context.Service.GetAllUsersAsync(
            new PaginationRequest { Page = 1, PageSize = 10 },
            keyword: null,
            isActive: true);

        Assert.True(response.Success);
        Assert.Equal(2, response.TotalCount);
        var data = Assert.IsType<GetAllUsersResponse>(response.Data);
        Assert.Equal(2, data.Users.Count);
        Assert.Equal(second.Id, data.Users[0].Id);
        Assert.DoesNotContain(data.Users, user => user.Id == adminCustomer.Id);
    }

    [Fact]
    public async Task GetAllUsersAsync_WhenKeywordProvided_FiltersNullSafe()
    {
        var context = new UserServiceTestContext();
        var matching = context.AddCustomer(email: null, phone: "+201111111111");
        matching.FirstName = "Ali";
        var other = context.AddCustomer(email: null, phone: null);
        other.FirstName = "Omar";

        var response = await context.Service.GetAllUsersAsync(
            new PaginationRequest { Page = 1, PageSize = 10 },
            keyword: "Ali",
            isActive: null);

        Assert.True(response.Success);
        var data = Assert.IsType<GetAllUsersResponse>(response.Data);
        var user = Assert.Single(data.Users);
        Assert.Equal(matching.Id, user.Id);
    }

    [Fact]
    public async Task AdminGetUserByIdAsync_WhenUserMissing_ReturnsNotFound()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.AdminGetUserByIdAsync(Guid.NewGuid());

        Assert.False(response.Success);
        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public async Task AdminGetUserByIdAsync_WhenUserExists_ReturnsProfileWithRoles()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: "user@test.com");
        user.ImagePath = "profiles/user.png";

        var response = await context.Service.AdminGetUserByIdAsync(user.Id);

        Assert.True(response.Success);
        var data = Assert.IsType<GetUserByIdResponse>(response.Data);
        Assert.Equal(user.Id, data.Id);
        Assert.Equal("https://files.test/profiles/user.png", data.ImagePath);
        Assert.Contains("Customer", data.Roles);
    }

    [Fact]
    public async Task GetCurrentAdminContextAsync_WhenNoCurrentUser_ReturnsUnauthorized()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.GetCurrentAdminContextAsync();

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentAdminContextAsync_WhenCurrentUserIsInactive_ReturnsUnauthorized()
    {
        var context = new UserServiceTestContext();
        var admin = context.AddAdmin(isActive: false);
        context.CurrentUserService.CurrentUserId = admin.Id;

        var response = await context.Service.GetCurrentAdminContextAsync();

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
    }

    [Fact]
    public async Task GetCurrentAdminContextAsync_WhenAdminIsValid_ReturnsRolesAndActivePermissionsOnly()
    {
        var context = new UserServiceTestContext();
        var admin = context.AddAdmin();
        admin.ImagePath = "profiles/admin.png";
        context.CurrentUserService.CurrentUserId = admin.Id;
        context.AddRolePermission("Admin", Permissions.Users.View, roleActive: true);
        context.AddRolePermission("Admin", Permissions.Users.Edit, roleActive: true);
        context.AddRolePermission("Admin", Permissions.Users.Delete, roleActive: false);

        var response = await context.Service.GetCurrentAdminContextAsync();

        Assert.True(response.Success);
        var data = Assert.IsType<AdminCurrentUserResponse>(response.Data);
        Assert.Equal(admin.Id, data.Id);
        Assert.Contains("Admin", data.Roles);
        Assert.Contains(Permissions.Users.View, data.Permissions);
        Assert.Contains(Permissions.Users.Edit, data.Permissions);
        Assert.DoesNotContain(Permissions.Users.Delete, data.Permissions);
        Assert.Equal("https://files.test/profiles/admin.png", data.ImagePath);
    }

    [Fact]
    public async Task AdminResetUserPasswordAsync_WhenNoCurrentAdmin_ReturnsUnauthorized()
    {
        var context = new UserServiceTestContext();
        var target = context.AddCustomer();

        var response = await context.Service.AdminResetUserPasswordAsync(new AdminResetUserPasswordRequest
        {
            userId = target.Id,
            newPassword = "NewPass1"
        });

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
    }

    [Fact]
    public async Task AdminResetUserPasswordAsync_WhenCallerIsNotAdmin_ReturnsUnauthorized()
    {
        var context = new UserServiceTestContext();
        var caller = context.AddCustomer(email: "caller@test.com");
        var target = context.AddCustomer(email: "target@test.com");
        context.CurrentUserService.CurrentUserId = caller.Id;

        var response = await context.Service.AdminResetUserPasswordAsync(new AdminResetUserPasswordRequest
        {
            userId = target.Id,
            newPassword = "NewPass1"
        });

        Assert.False(response.Success);
        Assert.Equal(401, response.StatusCode);
    }

    [Fact]
    public async Task AdminResetUserPasswordAsync_WhenTargetMissing_ReturnsNotFound()
    {
        var context = new UserServiceTestContext();
        var admin = context.AddAdmin();
        context.AddActiveRole(Roles.Admin.ToString());
        context.CurrentUserService.CurrentUserId = admin.Id;

        var response = await context.Service.AdminResetUserPasswordAsync(new AdminResetUserPasswordRequest
        {
            userId = Guid.NewGuid(),
            newPassword = "NewPass1"
        });

        Assert.False(response.Success);
        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public async Task AdminResetUserPasswordAsync_WhenResetFails_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();
        var admin = context.AddAdmin();
        context.AddActiveRole(Roles.Admin.ToString());
        var target = context.AddCustomer(email: "target@test.com");
        context.CurrentUserService.CurrentUserId = admin.Id;
        context.ResetPasswordResult = IdentityResult.Failed(new IdentityError { Description = "weak" });

        var response = await context.Service.AdminResetUserPasswordAsync(new AdminResetUserPasswordRequest
        {
            userId = target.Id,
            newPassword = "weak"
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
    }

    [Fact]
    public async Task AdminResetUserPasswordAsync_WhenSuccessful_UpdatesSecurityStampAndRevokesTokens()
    {
        var context = new UserServiceTestContext();
        var admin = context.AddAdmin();
        context.AddActiveRole(Roles.Admin.ToString());
        var target = context.AddCustomer(email: "target@test.com");
        var refreshToken = context.AddRefreshToken(target, "old", active: true);
        context.CurrentUserService.CurrentUserId = admin.Id;

        var response = await context.Service.AdminResetUserPasswordAsync(new AdminResetUserPasswordRequest
        {
            userId = target.Id,
            newPassword = "NewPass1"
        });

        Assert.True(response.Success);
        Assert.Equal("NewPass1", context.Passwords[target.Id]);
        Assert.True(refreshToken.IsRevoked);
        Assert.Equal("Password reset by admin.", refreshToken.ReasonRevoked);
        context.UserManager.Verify(manager => manager.UpdateSecurityStampAsync(target), Times.Once);
    }

    [Fact]
    public async Task AdminActivateUserEmailAsync_WhenUserMissing_ReturnsNotFound()
    {
        var context = new UserServiceTestContext();

        var response = await context.Service.AdminActivateUserEmailAsync(new AdminActivateUserEmailRequest
        {
            userId = Guid.NewGuid()
        });

        Assert.False(response.Success);
        Assert.Equal(404, response.StatusCode);
    }

    [Fact]
    public async Task AdminActivateUserEmailAsync_WhenUserHasNoEmail_ReturnsBadRequest()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: null, phone: "+201000000000");

        var response = await context.Service.AdminActivateUserEmailAsync(new AdminActivateUserEmailRequest
        {
            userId = user.Id
        });

        Assert.False(response.Success);
        Assert.Equal(400, response.StatusCode);
        Assert.Equal("Messages.UserNoEmail", response.Message);
    }

    [Fact]
    public async Task AdminActivateUserEmailAsync_WhenAlreadyConfirmed_ReturnsOk()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: "user@test.com", emailConfirmed: true);

        var response = await context.Service.AdminActivateUserEmailAsync(new AdminActivateUserEmailRequest
        {
            userId = user.Id
        });

        Assert.True(response.Success);
        Assert.Equal("Messages.EmailAlreadyVerified", response.Message);
    }

    [Fact]
    public async Task AdminActivateUserEmailAsync_WhenValid_ConfirmsEmail()
    {
        var context = new UserServiceTestContext();
        var user = context.AddCustomer(email: "user@test.com", emailConfirmed: false);

        var response = await context.Service.AdminActivateUserEmailAsync(new AdminActivateUserEmailRequest
        {
            userId = user.Id
        });

        Assert.True(response.Success);
        Assert.True(user.EmailConfirmed);
        context.UserManager.Verify(manager => manager.UpdateAsync(user), Times.Once);
    }
}


