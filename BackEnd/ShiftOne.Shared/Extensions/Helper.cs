using System.Globalization;
using System.Net.Mail;

namespace ShiftOne.Shared.Extensions
{
    public static class Helper
    {
        public static MailMessage AddRecipient(this MailMessage mailMessage, string toEmail)
        {
            mailMessage.To.Add(toEmail);
            return mailMessage;
        }        public static bool EqualsIgnoreCase(this string source, string other)
        {
            return string.Equals(source, other, StringComparison.OrdinalIgnoreCase);
        }
        public static string ToTitleCaseInvariant(this string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            var culture = CultureInfo.InvariantCulture;
            return culture.TextInfo.ToTitleCase(input.ToLowerInvariant());
        }               
    }
}



