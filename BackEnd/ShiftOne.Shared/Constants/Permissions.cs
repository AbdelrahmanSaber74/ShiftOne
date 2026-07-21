namespace ShiftOne.Shared.Constants
{
    public static class Permissions
    {
        public const string PolicyPrefix = "Permission";

        public static IReadOnlyCollection<string> All { get; } =
        [
            Users.View,
            Users.Approve,
            Users.Edit,
            Users.Delete,
            Roles.View,
            Roles.Create,
            Roles.Edit,
            Roles.Delete,
            PermissionManagement.View,
            PermissionManagement.Create,
            PermissionManagement.Edit,
            PermissionManagement.Delete,
            Profile.View,
            Profile.Edit,
            Profile.Delete,
            Plans.View,
            Plans.Create,
            Plans.Edit,
            Plans.Delete,
            Companies.View,
            Companies.Create,
            Companies.Edit,
            Companies.Delete,
            Subscriptions.View,
            Subscriptions.Create,
            Subscriptions.Edit,
            Branches.View,
            Branches.Create,
            Branches.Edit,
            Branches.Delete,
            Employees.View,
            Employees.Create,
            Employees.Edit,
            Employees.Delete,
            Attendance.View,
            Attendance.CheckIn,
            Attendance.CheckOut,
            Reports.View,
            Reports.Export,
            WorkSchedules.View,
            WorkSchedules.Create,
            WorkSchedules.Edit,
            WorkSchedules.Delete,
            WorkSchedules.Assign,
            Devices.Reset,
            System.Manage
        ];

        public static class Users
        {
            public const string View = "Users.View";
            public const string Approve = "Users.Approve";
            public const string Edit = "Users.Edit";
            public const string Delete = "Users.Delete";
        }

        public static class Roles
        {
            public const string View = "Roles.View";
            public const string Create = "Roles.Create";
            public const string Edit = "Roles.Edit";
            public const string Delete = "Roles.Delete";
        }

        public static class PermissionManagement
        {
            public const string View = "Permissions.View";
            public const string Create = "Permissions.Create";
            public const string Edit = "Permissions.Edit";
            public const string Delete = "Permissions.Delete";
        }

        public static class Profile
        {
            public const string View = "Profile.View";
            public const string Edit = "Profile.Edit";
            public const string Delete = "Profile.Delete";
        }

        public static class Plans
        {
            public const string View = "Plans.View";
            public const string Create = "Plans.Create";
            public const string Edit = "Plans.Edit";
            public const string Delete = "Plans.Delete";
        }

        public static class Companies
        {
            public const string View = "Companies.View";
            public const string Create = "Companies.Create";
            public const string Edit = "Companies.Edit";
            public const string Delete = "Companies.Delete";
        }

        public static class Subscriptions
        {
            public const string View = "Subscriptions.View";
            public const string Create = "Subscriptions.Create";
            public const string Edit = "Subscriptions.Edit";
        }

        public static class Branches
        {
            public const string View = "Branches.View";
            public const string Create = "Branches.Create";
            public const string Edit = "Branches.Edit";
            public const string Delete = "Branches.Delete";
        }

        public static class Employees
        {
            public const string View = "Employees.View";
            public const string Create = "Employees.Create";
            public const string Edit = "Employees.Edit";
            public const string Delete = "Employees.Delete";
        }

        public static class Attendance
        {
            public const string View = "Attendance.View";
            public const string CheckIn = "Attendance.CheckIn";
            public const string CheckOut = "Attendance.CheckOut";
        }

        public static class Reports
        {
            public const string View = "Reports.View";
            public const string Export = "Reports.Export";
        }

        public static class WorkSchedules
        {
            public const string View = "WorkSchedules.View";
            public const string Create = "WorkSchedules.Create";
            public const string Edit = "WorkSchedules.Edit";
            public const string Delete = "WorkSchedules.Delete";
            public const string Assign = "WorkSchedules.Assign";
        }

        public static class Devices
        {
            public const string Reset = "Devices.Reset";
        }

        public static class System
        {
            public const string Manage = "System.Manage";
        }
    }
}

