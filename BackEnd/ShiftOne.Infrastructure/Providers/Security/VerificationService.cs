using Microsoft.Extensions.Caching.Memory;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace ShiftOne.Infrastructure.Providers.Security
{
    public class VerificationService : IVerificationService
    {
        private const int MaxVerificationAttempts = 5;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;

        public VerificationService(IEmailService emailService, IMemoryCache cache)
        {
            _emailService = emailService;
            _cache = cache;
        }

        public async Task SendVerificationCodeAsync(Guid userId, string email, string subject, string lang)
        {
            var verificationCode = GenerateVerificationCode();

            var result = await _emailService.SendEmailAsync(email, subject, verificationCode, lang);
            if (result)
            {
                var entry = new VerificationCodeEntry(HashCode(verificationCode), 0);
                _cache.Set(GetCacheKey(userId), entry, TimeSpan.FromMinutes(10));
            }
            else
            {
                throw new Exception("Send verification code failed");
            }
        }

        public async Task SendRestpageUrlAsync(string email, string subject, string token, string pageUrl, string lang)
        {
            var encodedToken = WebUtility.UrlEncode(token);
            var url = $"{pageUrl}{email}?resetToken={encodedToken}";

            var result = await _emailService.SendEmailAsync(email, subject, url, lang, true);
            if (!result)
            {
                throw new Exception("Send reset url failed");
            }
        }

        public Task<bool> VerifyCodeAsync(Guid userId, string code)
        {
            var cacheKey = GetCacheKey(userId);
            var storedCode = _cache.Get<VerificationCodeEntry>(cacheKey);

            if (storedCode == null)
            {
                return Task.FromResult(false);
            }

            if (storedCode.Attempts >= MaxVerificationAttempts)
            {
                _cache.Remove(cacheKey);
                return Task.FromResult(false);
            }

            if (SecureEquals(storedCode.CodeHash, HashCode(code)))
            {
                _cache.Remove(cacheKey);
                return Task.FromResult(true);
            }

            _cache.Set(cacheKey, storedCode with { Attempts = storedCode.Attempts + 1 }, TimeSpan.FromMinutes(10));
            return Task.FromResult(false);
        }

        private static string GenerateVerificationCode()
        {
            return RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        }

        private static string GetCacheKey(Guid userId) => $"VerificationCode_{userId}";

        private static string HashCode(string code)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
            return Convert.ToBase64String(bytes);
        }

        private static bool SecureEquals(string left, string right)
        {
            var leftBytes = Convert.FromBase64String(left);
            var rightBytes = Convert.FromBase64String(right);

            return leftBytes.Length == rightBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(leftBytes, rightBytes);
        }

        private sealed record VerificationCodeEntry(string CodeHash, int Attempts);
    }
}
