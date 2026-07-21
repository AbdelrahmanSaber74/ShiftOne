namespace ShiftOne.Core.Common.Constants
{
    public static class AppConstants
    {
        public static class File
        {
            public const long MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
            public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        }

        public static class Auth
        {
            public const int RefreshTokenExpiryDays = 30;
            public const int VerificationCodeLength = 6;
            public const int MaxVerificationAttempts = 5;
            public const int VerificationCodeTtlMinutes = 10;
            public const int LoginDelayMs = 200;
            public const int RateLimitRequestsPerMinute = 1000;
        }

        public static class Claims
        {
            public const string UserIdentifier = "UserIdentifier";
            public const string IsActive = "IsActive";
            public const string SecurityStamp = "SecurityStamp";
            public const string CompanyId = "CompanyId";
            public const string BranchId = "BranchId";
        }

        public static class Pagination
        {
            public const int DefaultPage = 1;
            public const int DefaultPageSize = 20;
        }
    }
}


