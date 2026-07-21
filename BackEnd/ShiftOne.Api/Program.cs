using Serilog;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Collections.ObjectModel;
using System.Data;
using ShiftOne.Api;
using ShiftOne.Api.Constants;
using ShiftOne.Api.Middleware;
using ShiftOne.Application;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Infrastructure;
using ShiftOne.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, loggerConfiguration) =>
{
    loggerConfiguration
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.MSSqlServer(
            connectionString: context.Configuration.GetConnectionString("DefaultConnection"),
            sinkOptions: new MSSqlServerSinkOptions
            {
                TableName = "Logs",
                AutoCreateSqlTable = false
            },
            restrictedToMinimumLevel: LogEventLevel.Information,
            columnOptions: CreateSqlLogColumnOptions())
        .ReadFrom.Services(services);
});

static ColumnOptions CreateSqlLogColumnOptions()
{
    var columnOptions = new ColumnOptions();

    columnOptions.Store.Remove(StandardColumn.MessageTemplate);
    columnOptions.TimeStamp.ColumnName = "Timestamp";
    columnOptions.TimeStamp.ConvertToUtc = true;
    columnOptions.Level.DataLength = 32;

    columnOptions.AdditionalColumns = new Collection<SqlColumn>
    {
        new() { ColumnName = "CorrelationId", DataType = SqlDbType.NVarChar, DataLength = 64, AllowNull = true },
        new() { ColumnName = "RequestMethod", DataType = SqlDbType.NVarChar, DataLength = 16, AllowNull = true },
        new() { ColumnName = "RequestPath", DataType = SqlDbType.NVarChar, DataLength = 2048, AllowNull = true },
        new() { ColumnName = "StatusCode", DataType = SqlDbType.Int, AllowNull = true },
        new() { ColumnName = "ExecutionTimeMs", DataType = SqlDbType.BigInt, AllowNull = true },
        new() { ColumnName = "ClientIp", DataType = SqlDbType.NVarChar, DataLength = 64, AllowNull = true },
        new() { ColumnName = "ApplicationName", DataType = SqlDbType.NVarChar, DataLength = 128, AllowNull = true },
        new() { ColumnName = "EnvironmentName", DataType = SqlDbType.NVarChar, DataLength = 64, AllowNull = true },
        new() { ColumnName = "MachineName", DataType = SqlDbType.NVarChar, DataLength = 128, AllowNull = true }
    };

    return columnOptions;
}

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApiServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRateLimiter();

if (app.Configuration.GetValue("Database:RunMigrationsOnStartup", app.Environment.IsDevelopment()))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (db.Database.IsRelational())
    {
        await db.Database.MigrateAsync();
    }
}

if (app.Configuration.GetValue("Database:SeedOnStartup", app.Environment.IsDevelopment()))
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
    var unitOfWork = services.GetRequiredService<IUnitOfWork>();

    await ApplicationDbSeeder.SeedAsync(roleManager, userManager, unitOfWork, app.Configuration);
}
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});

if (app.Environment.IsDevelopment() || app.Environment.IsProduction() || app.Environment.IsStaging())
//if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/swagger/{ApiConstants.Swagger.DashboardDocument}/swagger.json", ApiConstants.Swagger.DashboardTitle);
        options.SwaggerEndpoint($"/swagger/{ApiConstants.Swagger.EmployeesDocument}/swagger.json", ApiConstants.Swagger.EmployeesTitle);
        options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);        

        options.DocumentTitle = ApiConstants.Swagger.DocumentTitle;      
    });
}

app.MapGet("/", () => Results.Redirect("/swagger")).AllowAnonymous();

app.UseCors(ApiConstants.Cors.AllowSpecificOriginsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
