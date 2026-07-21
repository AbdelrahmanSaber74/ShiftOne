using ShiftOne.Shared.Constants;

namespace ShiftOne.Tests.Authorization
{
    public class SaasRolePermissionTests
    {
        [Fact]
        public void Roles_IncludeRequiredSaasRoles()
        {
            var roles = Enum.GetNames(typeof(Roles));

            Assert.Contains(nameof(Roles.SuperAdmin), roles);
            Assert.Contains(nameof(Roles.CompanyAdmin), roles);
            Assert.Contains(nameof(Roles.HR), roles);
            Assert.Contains(nameof(Roles.Employee), roles);
        }

        [Fact]
        public void Permissions_IncludeSaasManagementPermissions()
        {
            Assert.Contains(Permissions.Plans.View, Permissions.All);
            Assert.Contains(Permissions.Companies.Create, Permissions.All);
            Assert.Contains(Permissions.Branches.Create, Permissions.All);
            Assert.Contains(Permissions.Employees.Create, Permissions.All);
            Assert.Contains(Permissions.Attendance.CheckIn, Permissions.All);
            Assert.Contains(Permissions.Devices.Reset, Permissions.All);
            Assert.Contains(Permissions.Roles.View, Permissions.All);
            Assert.Contains(Permissions.PermissionManagement.View, Permissions.All);
        }

        [Fact]
        public void CompanyAdmin_HasTenantOperatorPermissionsOnly()
        {
            Assert.Contains(Permissions.Branches.Create, TenantRolePermissions.CompanyAdmin);
            Assert.Contains(Permissions.Branches.Delete, TenantRolePermissions.CompanyAdmin);
            Assert.Contains(Permissions.Employees.Delete, TenantRolePermissions.CompanyAdmin);
            Assert.Contains(Permissions.Reports.Export, TenantRolePermissions.CompanyAdmin);
            Assert.DoesNotContain(Permissions.Companies.View, TenantRolePermissions.CompanyAdmin);
            Assert.DoesNotContain(Permissions.Plans.View, TenantRolePermissions.CompanyAdmin);
            Assert.DoesNotContain(Permissions.Subscriptions.View, TenantRolePermissions.CompanyAdmin);
            Assert.DoesNotContain(Permissions.Roles.View, TenantRolePermissions.CompanyAdmin);
            Assert.DoesNotContain(Permissions.PermissionManagement.View, TenantRolePermissions.CompanyAdmin);
            Assert.DoesNotContain(Permissions.System.Manage, TenantRolePermissions.CompanyAdmin);
        }

        [Fact]
        public void Hr_HasEmployeeOperationsWithoutBranchMutationOrDelete()
        {
            Assert.Contains(Permissions.Branches.View, TenantRolePermissions.HR);
            Assert.Contains(Permissions.Employees.Create, TenantRolePermissions.HR);
            Assert.Contains(Permissions.Employees.Edit, TenantRolePermissions.HR);
            Assert.Contains(Permissions.Devices.Reset, TenantRolePermissions.HR);
            Assert.Contains(Permissions.Reports.Export, TenantRolePermissions.HR);
            Assert.DoesNotContain(Permissions.Branches.Create, TenantRolePermissions.HR);
            Assert.DoesNotContain(Permissions.Branches.Edit, TenantRolePermissions.HR);
            Assert.DoesNotContain(Permissions.Branches.Delete, TenantRolePermissions.HR);
            Assert.DoesNotContain(Permissions.Employees.Delete, TenantRolePermissions.HR);
            Assert.DoesNotContain(Permissions.Companies.View, TenantRolePermissions.HR);
            Assert.DoesNotContain(Permissions.Roles.View, TenantRolePermissions.HR);
            Assert.DoesNotContain(Permissions.PermissionManagement.View, TenantRolePermissions.HR);
        }

        private static class TenantRolePermissions
        {
            public static readonly string[] CompanyAdmin =
            [
                Permissions.Profile.View,
                Permissions.Profile.Edit,
                Permissions.Branches.View,
                Permissions.Branches.Create,
                Permissions.Branches.Edit,
                Permissions.Branches.Delete,
                Permissions.Employees.View,
                Permissions.Employees.Create,
                Permissions.Employees.Edit,
                Permissions.Employees.Delete,
                Permissions.Attendance.View,
                Permissions.Reports.View,
                Permissions.Reports.Export,
                Permissions.Devices.Reset
            ];

            public static readonly string[] HR =
            [
                Permissions.Profile.View,
                Permissions.Profile.Edit,
                Permissions.Branches.View,
                Permissions.Employees.View,
                Permissions.Employees.Create,
                Permissions.Employees.Edit,
                Permissions.Attendance.View,
                Permissions.Reports.View,
                Permissions.Reports.Export,
                Permissions.Devices.Reset
            ];
        }
    }
}
