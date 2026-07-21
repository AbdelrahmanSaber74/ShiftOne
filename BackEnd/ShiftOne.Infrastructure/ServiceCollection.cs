using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Infrastructure.Persistence;
using ShiftOne.Infrastructure.Providers.Configurations;
using ShiftOne.Infrastructure.Providers.Email;
using ShiftOne.Infrastructure.Providers.Files;
using ShiftOne.Infrastructure.Providers.Security;
using ShiftOne.Infrastructure.Providers.Security.Permissions;
using ShiftOne.Infrastructure.Providers.Localization;
using ShiftOne.Infrastructure.Repositories;
using ShiftOne.Infrastructure.Reports;
using ShiftOne.Infrastructure.Caching;
using ShiftOne.Core.Interfaces.Infrastructure.Reports;
using ShiftOne.Core.Interfaces.Infrastructure.Caching;
using ShiftOne.Shared.Constants;
using ShiftOne.Core.Common.Constants;
using System.Text;
using System.Threading.RateLimiting;

namespace ShiftOne.Infrastructure
{
    public static class ServiceCollection
    {        
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            ValidateJwtSettings();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(AppSettings.Instance.ConnectionString);                
            });
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = true;
                options.Password.RequiredLength = 8;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.MaxFailedAccessAttempts = 5;

                options.User.RequireUniqueEmail = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromMinutes(10);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = AppSettings.Instance.JwtSettings.Issuer,
                        ValidAudience = AppSettings.Instance.JwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppSettings.Instance.JwtSettings.SecretKey))
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            // Read token from query for hub endpoints
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                path.StartsWithSegments("/signalRHubs")) // <-- your hub path
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                        ,
                        OnTokenValidated = async context =>
                        {
                            var userIdClaim = context.Principal?.FindFirst(AppConstants.Claims.UserIdentifier)?.Value;
                            var securityStampClaim = context.Principal?.FindFirst(AppConstants.Claims.SecurityStamp)?.Value;

                            if (!Guid.TryParse(userIdClaim, out var userId))
                            {
                                context.Fail("Invalid token.");
                                return;
                            }

                            var dbContext = context.HttpContext.RequestServices.GetRequiredService<ApplicationDbContext>();
                            var user = await dbContext.ApplicationUsers
                                .AsNoTracking()
                                .FirstOrDefaultAsync(applicationUser => applicationUser.Id == userId);

                            if (user == null ||
                                !user.IsActive ||
                                user.LockoutEnd > DateTimeOffset.UtcNow ||
                                !string.Equals(user.SecurityStamp, securityStampClaim, StringComparison.Ordinal))
                            {
                                context.Fail("Invalid token.");
                            }
                        }
                    };
                });

            services.AddAuthorization(options =>
            {
                var authenticatedPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                options.DefaultPolicy = authenticatedPolicy;
                options.FallbackPolicy = authenticatedPolicy;

                foreach (var permission in Permissions.All)
                {
                    options.AddPolicy(
                        $"{Permissions.PolicyPrefix}:{permission}",
                        policy => policy.Requirements.Add(new PermissionRequirement(permission)));
                }
            });

            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddMemoryCache();
            services.AddHttpClient();
            services.AddSignalR();

            services.AddScoped<IJwtService, JwtService>();
            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString()
                             ?? "unknown";
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ip,
                        factory: partition => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 1000,
                            Window = TimeSpan.FromMinutes(1),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                });
            });
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IVerificationService, VerificationService>();
            services.AddScoped<IFileService, FileService>();
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantContext, TenantContext>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ILocalizationService, LocalizationService>();
            services.AddSingleton<ICacheVersionManager, CacheVersionManager>();
            services.AddSingleton<ICacheKeyBuilder, CacheKeyBuilder>();
            services.AddSingleton<ICacheService, CacheService>();
            services.AddScoped<ICacheInvalidationService, CacheInvalidationService>();
            services.AddScoped<IReportQueryProvider, ReportQueryProvider>();
            services.AddScoped<IReportExportService, ReportExportService>();
            services.AddScoped<IExcelExportService, ExcelExportService>();
            services.AddScoped<IPdfExportService, PdfExportService>();

            return services;
        }

        private static void ValidateJwtSettings()
        {
            var jwtSettings = AppSettings.Instance.JwtSettings;
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

            if (string.IsNullOrWhiteSpace(jwtSettings.SecretKey) ||
                Encoding.UTF8.GetByteCount(jwtSettings.SecretKey) < 32)
            {
                throw new InvalidOperationException("JwtSettings:SecretKey must be at least 32 bytes.");
            }

            if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase) &&
                jwtSettings.SecretKey.Contains("local-development", StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Production cannot use the local development JWT secret.");
            }
        }

    }
}



