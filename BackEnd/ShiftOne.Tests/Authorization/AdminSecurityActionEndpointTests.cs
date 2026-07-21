using Microsoft.AspNetCore.Mvc;
using ShiftOne.Api.Authorization;
using ShiftOne.Api.Controllers.DashboardEndPoints.Admin;
using ShiftOne.Shared.Constants;
using ShiftOne.Shared.Requests.User;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ShiftOne.Tests.Authorization
{
    public class AdminSecurityActionEndpointTests
    {
        [Fact]
        public void ResetUserPasswordEndpoint_UsesExpectedRouteAndPermission()
        {
            var method = typeof(AdminController).GetMethod(nameof(AdminController.ResetUserPasswordAsync));

            Assert.NotNull(method);
            var route = method!.GetCustomAttribute<HttpPatchAttribute>();
            var permission = method.GetCustomAttribute<HasPermissionAttribute>();

            Assert.Equal("reset-password", route?.Template);
            Assert.Equal($"{Permissions.PolicyPrefix}:{Permissions.Users.Edit}", permission?.Policy);
        }

        [Fact]
        public void ActivateUserEmailEndpoint_UsesExpectedRouteAndPermission()
        {
            var method = typeof(AdminController).GetMethod(nameof(AdminController.ActivateUserEmailAsync));

            Assert.NotNull(method);
            var route = method!.GetCustomAttribute<HttpPatchAttribute>();
            var permission = method.GetCustomAttribute<HasPermissionAttribute>();

            Assert.Equal("activate-email", route?.Template);
            Assert.Equal($"{Permissions.PolicyPrefix}:{Permissions.Users.Edit}", permission?.Policy);
        }

        [Fact]
        public void AdminResetUserPasswordRequest_RequiresStrongPassword()
        {
            var request = new AdminResetUserPasswordRequest
            {
                newPassword = "weak"
            };
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                request,
                new ValidationContext(request),
                validationResults,
                validateAllProperties: true);

            Assert.False(isValid);
        }

        [Fact]
        public void AdminActivateUserEmailRequest_RequiresUserId()
        {
            var request = new AdminActivateUserEmailRequest();
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                request,
                new ValidationContext(request),
                validationResults,
                validateAllProperties: true);

            Assert.False(isValid);
        }
    }
}
