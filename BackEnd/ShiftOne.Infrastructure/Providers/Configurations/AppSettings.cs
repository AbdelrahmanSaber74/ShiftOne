using Microsoft.Extensions.Configuration;

namespace ShiftOne.Infrastructure.Providers.Configurations
{
    public class AppSettings
    {
        private static readonly Lazy<AppSettings> _instance = new(() => new AppSettings());

        public static AppSettings Instance => _instance.Value;

        public JwtSettings JwtSettings { get; private set; } = new();
        public SmtpSettings SmtpSettings { get; private set; } = new();
        public string ConnectionString { get; private set; } = string.Empty;

        private AppSettings()
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            JwtSettings = configuration.GetSection("JwtSettings").Get<JwtSettings>() ?? new JwtSettings();
            SmtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>() ?? new SmtpSettings();

            JwtSettings.SecretKey = Environment.GetEnvironmentVariable("SecretKey") ?? JwtSettings.SecretKey;
            SmtpSettings.SenderPassword = Environment.GetEnvironmentVariable("SenderPassword") ?? SmtpSettings.SenderPassword;
            ConnectionString = ResolveConnectionString(configuration);
        }

        private static string ResolveConnectionString(IConfiguration configuration)
        {
            return Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                ?? Environment.GetEnvironmentVariable("DefaultConnection")
                ?? configuration.GetConnectionString("DefaultConnection")
                ?? string.Empty;
        }
    }
}