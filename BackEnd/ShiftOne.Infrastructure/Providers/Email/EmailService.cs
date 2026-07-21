using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Infrastructure.Providers.Configurations;
using ShiftOne.Shared.Extensions;
using System.Net;
using System.Net.Mail;

namespace ShiftOne.Infrastructure.Providers.Email
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService()
        {
            _smtpSettings = AppSettings.Instance.SmtpSettings;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string code, string lang, bool isRest = false)
        {
            var smtpClient = CreateSmtpClient();            
            string htmlContent = emailContent(lang, isRest,code);
            var mailMessage = CreateMailMessage(toEmail, subject, htmlContent);
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                await Console.Out.WriteLineAsync(ex.Message);
                return false;
            }
        }

        private SmtpClient CreateSmtpClient()
        {            
            return new SmtpClient
            {
                Host = _smtpSettings.Host,
                Port = _smtpSettings.Port,
                EnableSsl = _smtpSettings.EnableSsl,
                Credentials = new NetworkCredential(_smtpSettings.SenderEmail, _smtpSettings.SenderPassword)                
            };
        }
        private string emailContent(string lang, bool isRest, string code)
        {
            if (isRest)
                return $"Your reset link is: {code}";
            
            return $"Your verification code is: {code}";
        }
        private MailMessage CreateMailMessage(string toEmail, string subject, string body)
        {
            return new MailMessage
            {
                From = new MailAddress(_smtpSettings.SenderEmail,"ShiftOne Support"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            }.AddRecipient(toEmail);
        }
    }
}


