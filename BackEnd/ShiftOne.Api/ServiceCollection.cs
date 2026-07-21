using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using ShiftOne.Api.Constants;
using ShiftOne.Api.Filters;
using ShiftOne.Api.Options;
using System.Reflection;

namespace ShiftOne.Api
{
    public static class ServiceCollection
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<LoggingOptions>(configuration.GetSection(LoggingOptions.SectionName));
            services.AddEndpointsApiExplorer();
            services.AddCors(options =>
            {
                options.AddPolicy(ApiConstants.Cors.AllowSpecificOriginsPolicy, builder =>
                {
                    var allowedOrigins = configuration["AllowedOrigins"]?.Split(',') ?? new[] { "http://localhost:3000" };
                    builder.WithOrigins(allowedOrigins)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // Register custom attributes
            //services.AddScoped<ApiKeyAuthorizeAttribute>(); // Register ApiKeyAuthorize attribute as Scoped

            // API Versioning Setup
            //services.AddApiVersioning(options =>
            //{
            //    options.DefaultApiVersion = new ApiVersion(1, 0); // Set default API version
            //    options.AssumeDefaultVersionWhenUnspecified = true; // Use default if version isn't specified
            //    options.ReportApiVersions = true; // Return API version information in response headers
            //    options.ApiVersionReader = new HeaderApiVersionReader("x-api-version"); // Read API version from request headers
            //});

            //// Swagger Setup
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressMapClientErrors = true;
            });
            
            services.AddSwaggerGen(option =>
            {
                option.SchemaFilter<EnumSchemaFilter>();
                option.OperationFilter<AcceptLanguageHeaderFilter>();
                // Define different API docs for various apps
                option.SwaggerDoc(ApiConstants.Swagger.DashboardDocument, new OpenApiInfo
                {
                    Title = ApiConstants.Swagger.DashboardTitle,
                    Version = "v1",
                    //Contact = new OpenApiContact
                    //{
                    //    Name = "ShiftOne",
                    //    Email = "info@ShiftOne-platform.com",
                    //    Url = new Uri("https://ShiftOne-platform.com")
                    //}
                });

                option.SwaggerDoc(ApiConstants.Swagger.EmployeesDocument, new OpenApiInfo
                {
                    Title = ApiConstants.Swagger.EmployeesTitle,
                    Version = "v1",
                    //Contact = new OpenApiContact
                    //{
                    //    Name = "ShiftOne",
                    //    Email = "info@ShiftOne-platform.com",
                    //    Url = new Uri("https://ShiftOne-platform.com")
                    //}
                });

                // Optional: Group by controller group name
                option.DocInclusionPredicate((docName, apiDesc) =>
                {
                    if (string.IsNullOrEmpty(apiDesc.GroupName))
                        return false;

                    return apiDesc.GroupName == docName;
                });

                option.IncludeXmlComments(xmlPath); // Include XML comments for Swagger UI

                //// Add API Key and Bearer Token authentication schemes to Swagger UI
                //option.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
                //{
                //    Description = "API Key needed to access the endpoints. x-api-key: My_API_Key",
                //    In = ParameterLocation.Header,
                //    Name = "x-api-key",
                //    Type = SecuritySchemeType.ApiKey,
                //    Scheme = "ApiKeyScheme"
                //});

                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer"
                });

                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    //{
                    //    new OpenApiSecurityScheme
                    //    {
                    //        Reference = new OpenApiReference
                    //        {
                    //            Type = ReferenceType.SecurityScheme,
                    //            Id = "ApiKey"
                    //        }
                    //    },
                    //    new string[] {}
                    //},
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });                
            });

            services.AddProblemDetails();
            services.AddRouting(options => options.LowercaseUrls = true);
            services.AddControllers(options =>
            {
                options.Filters.Add<EnterpriseCacheFilter>();
                options.Filters.Add<LocalizationResultFilter>();
            });

            return services;
        }

    }
}





