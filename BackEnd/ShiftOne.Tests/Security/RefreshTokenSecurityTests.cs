using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Infrastructure.Providers.Security;
using ShiftOne.Shared.Requests.User;
using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Tests.Security
{
    public class RefreshTokenSecurityTests
    {
        [Fact]
        public void HashRefreshToken_DoesNotReturnPlainTextToken()
        {
            var jwtService = new JwtService(null!, null!);
            var refreshToken = "super-secret-refresh-token";

            var hash = jwtService.HashRefreshToken(refreshToken);

            Assert.NotEqual(refreshToken, hash);
            Assert.False(string.IsNullOrWhiteSpace(hash));
        }

        [Fact]
        public void HashRefreshToken_IsDeterministicForLookup()
        {
            var jwtService = new JwtService(null!, null!);
            var refreshToken = "super-secret-refresh-token";

            Assert.Equal(
                jwtService.HashRefreshToken(refreshToken),
                jwtService.HashRefreshToken(refreshToken));
        }

        [Fact]
        public void RefreshToken_IsActiveOnlyWhenNotRevokedAndNotExpired()
        {
            var token = new RefreshToken
            {
                ExpiryDate = DateTime.UtcNow.AddMinutes(5),
                IsRevoked = false,
                RevokedOn = null
            };

            Assert.True(token.IsActive);

            token.IsRevoked = true;

            Assert.False(token.IsActive);
        }

        [Fact]
        public void RefreshTokenRequest_RequiresRefreshToken()
        {
            var request = new RefreshTokenRequest();
            var validationResults = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(
                request,
                new ValidationContext(request),
                validationResults,
                validateAllProperties: true);

            Assert.False(isValid);
            Assert.Contains(validationResults, result => result.MemberNames.Contains(nameof(RefreshTokenRequest.RefreshToken)));
        }
    }
}
