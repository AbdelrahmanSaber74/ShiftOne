using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using ShiftOne.Core.Entities.Identity;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Core.Common.Constants;
using ShiftOne.Infrastructure.Providers.Configurations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ShiftOne.Infrastructure.Providers.Security
{
    public class JwtService : IJwtService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public JwtService(IUnitOfWork unitOfWork, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _fileService = fileService;
        }

        public async Task<string> GenerateJwtToken(ApplicationUser user, UserManager<ApplicationUser> userManager)
        {
            try
            {
                var jwtSettings = AppSettings.Instance.JwtSettings; // Access singleton config instance                
                var claims = new List<Claim>
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(AppConstants.Claims.UserIdentifier, user.Id.ToString()),
                    new Claim(AppConstants.Claims.SecurityStamp, user.SecurityStamp ?? string.Empty),
                    new Claim(AppConstants.Claims.IsActive, user.IsActive.ToString()),
                };

                if (user.CompanyId.HasValue)
                {
                    claims.Add(new Claim(AppConstants.Claims.CompanyId, user.CompanyId.Value.ToString()));
                }

                if (user.BranchId.HasValue)
                {
                    claims.Add(new Claim(AppConstants.Claims.BranchId, user.BranchId.Value.ToString()));
                }

                var roles = await userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: jwtSettings.Issuer,
                    audience: jwtSettings.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(jwtSettings.ExpiryMinutes),
                    signingCredentials: creds);
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception)
            {

                throw new Exception("Generate token faild");
            }

        }

        public Task<string> GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            var token = Convert.ToBase64String(randomNumber);
            return Task.FromResult(token);
        }

        public string HashRefreshToken(string refreshToken)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
            return Convert.ToBase64String(bytes);
        }

        public async Task<bool> SaveRefreshToken(Guid userId, string refreshToken, string createdByIp = "")
        {
            try
            {
                await _unitOfWork.Repository<RefreshToken>().AddAsync(new RefreshToken()
                {
                    TokenHash = HashRefreshToken(refreshToken),
                    ApplicationUserId = userId,
                    ExpiryDate = DateTime.UtcNow.AddDays(30),
                    CreatedOn = DateTime.UtcNow,
                    CreatedByIp = createdByIp,
                    IsRevoked = false,
                });
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }
}



