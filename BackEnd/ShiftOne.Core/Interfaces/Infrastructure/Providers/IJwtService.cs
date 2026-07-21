using Microsoft.AspNetCore.Identity;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Shared.Responses;

namespace ShiftOne.Core.Interfaces.Infrastructure.Providers
{
    public interface IJwtService
    {
        Task<string> GenerateJwtToken(ApplicationUser customer, UserManager<ApplicationUser> userManager);
        string HashRefreshToken(string refreshToken);
        Task<string> GenerateRefreshToken();
        Task<bool> SaveRefreshToken(Guid userId, string refreshToken, string createdByIp = "");
    }
}


