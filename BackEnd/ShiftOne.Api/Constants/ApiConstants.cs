namespace ShiftOne.Api.Constants
{
    public static class ApiConstants
    {
        public static class Application
        {
            public const string Name = "ShiftOne.Api";
        }

        public static class Cors
        {
            public const string AllowSpecificOriginsPolicy = "AllowSpecificOrigins";
        }

        public static class Headers
        {
            public const string CorrelationId = "X-Correlation-ID";
        }

        public static class HttpContextItems
        {
            public const string CorrelationId = "CorrelationId";
        }

        public static class LogProperties
        {
            public const string ApplicationName = "ApplicationName";
            public const string ClientIp = "ClientIp";
            public const string CorrelationId = "CorrelationId";
            public const string EnvironmentName = "EnvironmentName";
            public const string ExecutionTimeMs = "ExecutionTimeMs";
            public const string RequestMethod = "RequestMethod";
            public const string RequestPath = "RequestPath";
            public const string StatusCode = "StatusCode";
            public const string TimestampUtc = "TimestampUtc";
        }

        public static class Swagger
        {
            public const string DashboardDocument = "dashboard";
            public const string EmployeesDocument = "employees";
            public const string DashboardTitle = "Dashboard API";
            public const string EmployeesTitle = "Employees API";
            public const string DocumentTitle = "ShiftOne API Docs";
        }
    }
}
