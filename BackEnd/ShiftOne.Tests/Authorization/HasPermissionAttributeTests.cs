using ShiftOne.Api.Authorization;
using ShiftOne.Shared.Constants;

namespace ShiftOne.Tests.Authorization
{
    public class HasPermissionAttributeTests
    {
        [Fact]
        public void Constructor_SetsExpectedPermissionPolicy()
        {
            var attribute = new HasPermissionAttribute(Permissions.Users.View);

            Assert.Equal("Permission:Users.View", attribute.Policy);
        }

        [Fact]
        public void AllPermissions_ContainsExpectedBootstrapPermissions()
        {
            Assert.Contains(Permissions.Users.View, Permissions.All);
            Assert.Contains(Permissions.Users.Approve, Permissions.All);
            Assert.Contains(Permissions.Users.Delete, Permissions.All);
            Assert.Contains(Permissions.Profile.View, Permissions.All);
            Assert.Contains(Permissions.Profile.Edit, Permissions.All);
            Assert.Contains(Permissions.Profile.Delete, Permissions.All);
        }
    }
}
