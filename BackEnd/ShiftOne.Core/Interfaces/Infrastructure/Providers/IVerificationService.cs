namespace ShiftOne.Core.Interfaces.Infrastructure.Providers
{
    public interface IVerificationService
    {
        Task SendVerificationCodeAsync(Guid userId, string email, string subject,string lang);
        Task SendRestpageUrlAsync(string email, string subject, string token, string pageUrl,string lang);
        Task<bool> VerifyCodeAsync(Guid userId, string code);
        Task<string> GenerateAndSavePhoneCodeAsync(Guid userId, string phone);
    }
}


