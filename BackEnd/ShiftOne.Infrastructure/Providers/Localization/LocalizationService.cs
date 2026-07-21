using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;

namespace ShiftOne.Infrastructure.Providers.Localization
{
    public class LocalizationService : ILocalizationService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _cache = new();
        private const string DefaultLanguage = "en";
        private static readonly HashSet<string> SupportedLanguages = new(StringComparer.OrdinalIgnoreCase) { "en", "ar" };

        public LocalizationService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetString(string key)
        {
            return GetString(key, null);
        }

        public string GetString(string key, Dictionary<string, string>? placeholders)
        {
            var lang = GetRequestLanguage();
            var translations = GetTranslationsForLanguage(lang);

            if (!translations.TryGetValue(key, out var translation))
            {
                // Fallback to default language if key not found in requested language
                if (lang != DefaultLanguage)
                {
                    var defaultTranslations = GetTranslationsForLanguage(DefaultLanguage);
                    if (defaultTranslations.TryGetValue(key, out translation))
                    {
                        return FormatMessage(translation, placeholders);
                    }
                }
                return key; // If key not found anywhere, return the key name
            }

            return FormatMessage(translation, placeholders);
        }

        private string GetRequestLanguage()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
            {
                return DefaultLanguage;
            }

            var acceptLanguageHeader = httpContext.Request.Headers["Accept-Language"].ToString();
            if (string.IsNullOrWhiteSpace(acceptLanguageHeader))
            {
                return DefaultLanguage;
            }

            // Parse Accept-Language header e.g. "ar-EG,ar;q=0.9,en-US;q=0.8,en;q=0.7"
            var parts = acceptLanguageHeader.Split(',');
            foreach (var part in parts)
            {
                var cleanPart = part.Split(';')[0].Trim(); // e.g. "ar-EG" or "ar"
                
                // Check if full locale is supported (e.g. "en") or if two-letter code matches (e.g. "ar" from "ar-EG")
                var twoLetter = cleanPart.Split('-')[0].ToLowerInvariant();

                if (SupportedLanguages.Contains(cleanPart))
                {
                    return cleanPart.ToLowerInvariant();
                }
                
                if (SupportedLanguages.Contains(twoLetter))
                {
                    return twoLetter;
                }
            }

            return DefaultLanguage;
        }

        private Dictionary<string, string> GetTranslationsForLanguage(string lang)
        {
            return _cache.GetOrAdd(lang, l =>
            {
                var filePath = Path.Combine(AppContext.BaseDirectory, "Localization", $"{l}.json");
                if (!File.Exists(filePath))
                {
                    return new Dictionary<string, string>();
                }

                try
                {
                    var json = File.ReadAllText(filePath);
                    var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                    return dict ?? new Dictionary<string, string>();
                }
                catch
                {
                    return new Dictionary<string, string>();
                }
            });
        }

        private string FormatMessage(string message, Dictionary<string, string>? placeholders)
        {
            if (placeholders == null || placeholders.Count == 0)
            {
                return message;
            }

            foreach (var placeholder in placeholders)
            {
                // Support both {Placeholder} and {placeholder}
                message = message.Replace($"{{{placeholder.Key}}}", placeholder.Value, StringComparison.OrdinalIgnoreCase);
            }

            return message;
        }
    }
}
