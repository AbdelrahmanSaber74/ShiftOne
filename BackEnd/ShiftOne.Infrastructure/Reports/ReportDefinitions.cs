using ShiftOne.Shared.Responses.Reports;

namespace ShiftOne.Infrastructure.Reports
{
    internal static class ReportDefinitions
    {
        public const string Attendance = "attendance";
        public const string Employees = "employees";
        public const string Companies = "companies";
        public const string Branches = "branches";
        public const string Subscriptions = "subscriptions";
        public const string PlanUsage = "plan-usage";

        public static IReadOnlyList<ReportColumn> Columns(string reportKey) => reportKey switch
        {
            Attendance => new[]
            {
                Col("companyName", "Company"), Col("branchName", "Branch"), Col("employeeName", "Employee"),
                Col("attendanceDate", "Date"), Col("status", "Status"), Col("holidayName", "Holiday"),
                Col("workScheduleName", "Schedule"),
                Col("scheduledStartTime", "Scheduled Start"), Col("scheduledEndTime", "Scheduled End"),
                Col("checkInAt", "Check-In"), Col("checkOutAt", "Check-Out"), Col("workedMinutes", "Worked Minutes"),
                Col("lateMinutes", "Late Minutes"), Col("earlyLeaveMinutes", "Early Leave Minutes"),
                Col("overtimeMinutes", "Overtime Minutes"), Col("deviceId", "Device ID")
            },
            Employees => new[]
            {
                Col("companyName", "Company"), Col("branchName", "Branch"), Col("employeeName", "Employee"),
                Col("email", "Email"), Col("phoneNumber", "Phone"), Col("roles", "Roles"),
                Col("isActive", "Status"), Col("joinedOn", "Joined On"), Col("hasBoundDevice", "Device Bound")
            },
            Companies => new[]
            {
                Col("companyName", "Company"), Col("planName", "Plan"), Col("branchesCount", "Branches"),
                Col("employeesCount", "Employees"), Col("subscriptionStatus", "Subscription Status"), Col("expirationDate", "Expiration Date")
            },
            Branches => new[]
            {
                Col("companyName", "Company"), Col("branchName", "Branch"), Col("employeesCount", "Employees"),
                Col("attendanceToday", "Attendance Today"), Col("geoFenceStatus", "GeoFence"), Col("isActive", "Status")
            },
            Subscriptions => new[]
            {
                Col("companyName", "Company"), Col("planName", "Plan"), Col("price", "Price"), Col("status", "Status"),
                Col("startDate", "Start Date"), Col("endDate", "End Date"), Col("remainingDays", "Remaining Days")
            },
            PlanUsage => new[]
            {
                Col("planName", "Plan"), Col("companiesCount", "Companies"), Col("employeesCount", "Employees"),
                Col("branchesCount", "Branches"), Col("averageUsage", "Average Usage")
            },
            _ => throw new KeyNotFoundException(reportKey)
        };

        public static string Title(string reportKey) => reportKey switch
        {
            Attendance => "Attendance Report",
            Employees => "Employee Report",
            Companies => "Company Report",
            Branches => "Branch Report",
            Subscriptions => "Subscription Report",
            PlanUsage => "Plan Usage Report",
            _ => throw new KeyNotFoundException(reportKey)
        };

        private static ReportColumn Col(string key, string header) => new() { Key = key, Header = header };
    }
}


