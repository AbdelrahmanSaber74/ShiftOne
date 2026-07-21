using System.Collections.Generic;

namespace ShiftOne.Core.Interfaces.Infrastructure.Providers
{
    public interface ILocalizationService
    {
        string GetString(string key);
        string GetString(string key, Dictionary<string, string>? placeholders);
    }
}
