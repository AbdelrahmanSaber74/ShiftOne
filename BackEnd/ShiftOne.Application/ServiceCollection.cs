using Microsoft.Extensions.DependencyInjection;
using ShiftOne.Application.Services.Saas;
using ShiftOne.Application.Services.WorkSchedules;
using ShiftOne.Application.Services.Security;
using ShiftOne.Application.Services.Reports;
using ShiftOne.Application.Services.User;
using ShiftOne.Core.Interfaces.Application;

namespace ShiftOne.Application
{
    public static class ServiceCollection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPlanLimitService, PlanLimitService>();
            services.AddScoped<ISubscriptionPlanService, SubscriptionPlanService>();
            services.AddScoped<ICompanyService, CompanyService>();
            services.AddScoped<ICompanySubscriptionService, CompanySubscriptionService>();
            services.AddScoped<IBranchService, BranchService>();
            services.AddScoped<IEmployeeManagementService, EmployeeManagementService>();
            services.AddScoped<IAttendanceService, AttendanceService>();
            services.AddScoped<IRoleManagementService, RoleManagementService>();
            services.AddScoped<IPermissionManagementService, PermissionManagementService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IWorkScheduleService, WorkScheduleService>();
            services.AddScoped<IAttendanceCalculationService, AttendanceCalculationService>();
            services.AddScoped<IAttendanceStatusResolver, AttendanceStatusResolver>();

            return services;
        }
    }
}