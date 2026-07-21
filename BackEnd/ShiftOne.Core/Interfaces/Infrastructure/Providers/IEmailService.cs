namespace ShiftOne.Core.Interfaces.Infrastructure.Providers
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string code, string lang,bool isRest = false);
    }
}


