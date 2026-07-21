using System.ComponentModel.DataAnnotations;

namespace ShiftOne.Shared.Validation
{
    public static class EmailOrPhoneValidator
    {
        public static EmailOrPhoneKind GetKind(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return EmailOrPhoneKind.Invalid;
            }

            var trimmedValue = value.Trim();
            if (new EmailAddressAttribute().IsValid(trimmedValue))
            {
                return EmailOrPhoneKind.Email;
            }

            if (new PhoneAttribute().IsValid(trimmedValue) && trimmedValue.Count(char.IsDigit) >= 7)
            {
                return EmailOrPhoneKind.Phone;
            }

            return EmailOrPhoneKind.Invalid;
        }

        public static string Normalize(string value)
        {
            return value.Trim();
        }
    }
}
