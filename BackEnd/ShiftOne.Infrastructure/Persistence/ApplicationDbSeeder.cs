using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using ShiftOne.Core.Entities.Holidays;
using ShiftOne.Core.Entities.Identity;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Shared.Constants;

namespace ShiftOne.Infrastructure.Persistence
{
    public static class ApplicationDbSeeder
    {
        public static async Task SeedAsync(RoleManager<ApplicationRole> roleManager, UserManager<ApplicationUser> userManager, IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            await SeedRolesAsync(roleManager);
            await SeedPermissionsAsync(unitOfWork);
            await SeedRolePermissionsAsync(roleManager, unitOfWork);
            await SeedUsersAsync(userManager, configuration);
            await SeedSaudiHolidaysAsync(unitOfWork);
        }

        private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
        {
            foreach (var role in Enum.GetNames(typeof(Roles)))
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole
                    {
                        Name = role,
                        IsActive = true,
                        Description = role
                    });
                }
            }
        }

        private static async Task SeedPermissionsAsync(IUnitOfWork unitOfWork)
        {
            var existingPermissions = (await unitOfWork.Repository<ApplicationPermission>().GetAllAsync())
                .Select(permission => permission.Name)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var permission in Permissions.All)
            {
                if (existingPermissions.Contains(permission))
                {
                    continue;
                }

                await unitOfWork.Repository<ApplicationPermission>().AddAsync(new ApplicationPermission
                {
                    Name = permission,
                    Description = permission
                });
            }

            await unitOfWork.CompleteAsync();
        }

        private static async Task SeedRolePermissionsAsync(RoleManager<ApplicationRole> roleManager, IUnitOfWork unitOfWork)
        {
            await AssignPermissionsAsync(roleManager, unitOfWork, Roles.SuperAdmin.ToString(), Permissions.All);
            await AssignPermissionsAsync(roleManager, unitOfWork, Roles.Admin.ToString(), Permissions.All);

            var companyAdminPermissions = new[]
            {
                Permissions.Profile.View,
                Permissions.Profile.Edit,
                Permissions.Branches.View,
                Permissions.Branches.Create,
                Permissions.Branches.Edit,
                Permissions.Branches.Delete,
                Permissions.Employees.View,
                Permissions.Employees.Create,
                Permissions.Employees.Edit,
                Permissions.Employees.Delete,
                Permissions.Attendance.View,
                Permissions.Reports.View,
                Permissions.Reports.Export,
                Permissions.WorkSchedules.View,
                Permissions.WorkSchedules.Create,
                Permissions.WorkSchedules.Edit,
                Permissions.WorkSchedules.Delete,
                Permissions.WorkSchedules.Assign,
                Permissions.Devices.Reset
            };
            await AssignPermissionsAsync(roleManager, unitOfWork, Roles.CompanyAdmin.ToString(), companyAdminPermissions);

            var hrPermissions = new[]
            {
                Permissions.Profile.View,
                Permissions.Profile.Edit,
                Permissions.Branches.View,
                Permissions.Employees.View,
                Permissions.Employees.Create,
                Permissions.Employees.Edit,
                Permissions.Attendance.View,
                Permissions.Reports.View,
                Permissions.Reports.Export,
                Permissions.WorkSchedules.View,
                Permissions.Devices.Reset
            };
            await AssignPermissionsAsync(roleManager, unitOfWork, Roles.HR.ToString(), hrPermissions);
            var employeePermissions = new[]
            {
                Permissions.Profile.View,
                Permissions.Profile.Edit,
                Permissions.Attendance.CheckIn,
                Permissions.Attendance.CheckOut
            };
            await AssignPermissionsAsync(roleManager, unitOfWork, Roles.Employee.ToString(), employeePermissions);
            await AssignPermissionsAsync(roleManager, unitOfWork, Roles.Customer.ToString(), employeePermissions);
        }

        private static async Task AssignPermissionsAsync(RoleManager<ApplicationRole> roleManager, IUnitOfWork unitOfWork, string roleName, IEnumerable<string> permissionNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role == null)
            {
                return;
            }

            var requested = permissionNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
            var permissions = (await unitOfWork.Repository<ApplicationPermission>().GetAllAsync())
                .Where(permission => requested.Contains(permission.Name))
                .ToList();
            var existingRolePermissions = (await unitOfWork.Repository<ApplicationRolePermission>().GetAllAsync())
                .Where(rolePermission => rolePermission.RoleId == role.Id)
                .Select(rolePermission => rolePermission.PermissionId)
                .ToHashSet();

            foreach (var permission in permissions)
            {
                if (existingRolePermissions.Contains(permission.Id))
                {
                    continue;
                }

                await unitOfWork.Repository<ApplicationRolePermission>().AddAsync(new ApplicationRolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permission.Id
                });
            }

            await unitOfWork.CompleteAsync();
        }


        private static async Task SeedSaudiHolidaysAsync(IUnitOfWork unitOfWork)
        {
            var holidays = new List<CompanyHoliday>();
            for (var year = 2026; year <= 2030; year++)
            {
                holidays.Add(new CompanyHoliday { Name = "Saudi Founding Day", Date = new DateTime(year, 2, 22), IsGlobal = true, IsActive = true });
                holidays.Add(new CompanyHoliday { Name = "Saudi National Day", Date = new DateTime(year, 9, 23), IsGlobal = true, IsActive = true });
            }

            holidays.AddRange(new[]
            {
                new CompanyHoliday { Name = "Eid Al-Fitr", Date = new DateTime(2026, 3, 20), IsGlobal = true, IsActive = true, Notes = "Initial 2026 seed; update after official moon sighting if needed." },
                new CompanyHoliday { Name = "Eid Al-Fitr", Date = new DateTime(2026, 3, 21), IsGlobal = true, IsActive = true, Notes = "Initial 2026 seed; update after official moon sighting if needed." },
                new CompanyHoliday { Name = "Eid Al-Fitr", Date = new DateTime(2026, 3, 22), IsGlobal = true, IsActive = true, Notes = "Initial 2026 seed; update after official moon sighting if needed." },
                new CompanyHoliday { Name = "Eid Al-Fitr", Date = new DateTime(2026, 3, 23), IsGlobal = true, IsActive = true, Notes = "Initial 2026 seed; update after official moon sighting if needed." },
                new CompanyHoliday { Name = "Arafah Day", Date = new DateTime(2026, 5, 26), IsGlobal = true, IsActive = true, Notes = "Initial 2026 seed; update after official moon sighting if needed." },
                new CompanyHoliday { Name = "Eid Al-Adha", Date = new DateTime(2026, 5, 27), IsGlobal = true, IsActive = true, Notes = "Initial 2026 seed; update after official moon sighting if needed." },
                new CompanyHoliday { Name = "Eid Al-Adha", Date = new DateTime(2026, 5, 28), IsGlobal = true, IsActive = true, Notes = "Initial 2026 seed; update after official moon sighting if needed." },
                new CompanyHoliday { Name = "Eid Al-Adha", Date = new DateTime(2026, 5, 29), IsGlobal = true, IsActive = true, Notes = "Initial 2026 seed; update after official moon sighting if needed." },
                new CompanyHoliday { Name = "Eid Al-Adha", Date = new DateTime(2026, 5, 30), IsGlobal = true, IsActive = true, Notes = "Initial 2026 seed; update after official moon sighting if needed." }
            });

            var existing = (await unitOfWork.Repository<CompanyHoliday>().GetAllAsync())
                .Where(holiday => holiday.CompanyId == null)
                .Select(holiday => new { holiday.Name, Date = holiday.Date.Date })
                .ToHashSet();

            foreach (var holiday in holidays)
            {
                var key = new { holiday.Name, Date = holiday.Date.Date };
                if (existing.Contains(key)) continue;
                await unitOfWork.Repository<CompanyHoliday>().AddAsync(holiday);
            }

            await unitOfWork.CompleteAsync();
        }
        private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            var section = configuration.GetSection("SuperAdmin");
            var email = section["Email"]?.Trim();
            var password = section["Password"];

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException("SuperAdmin:Email and SuperAdmin:Password must be configured when database seeding is enabled.");
            }

            var adminUser = await userManager.FindByEmailAsync(email);
            if (adminUser == null)
            {
                adminUser = new Admin
                {
                    UserName = string.IsNullOrWhiteSpace(section["UserName"]) ? email : section["UserName"]!.Trim(),
                    Email = email,
                    EmailConfirmed = true,
                    IsActive = true,
                    FirstName = string.IsNullOrWhiteSpace(section["FirstName"]) ? "Super" : section["FirstName"]!.Trim(),
                    LastName = string.IsNullOrWhiteSpace(section["LastName"]) ? "Admin" : section["LastName"]!.Trim(),
                    PhoneNumber = section["PhoneNumber"],
                    PhoneNumberConfirmed = !string.IsNullOrWhiteSpace(section["PhoneNumber"])
                };

                var result = await userManager.CreateAsync(adminUser, password);
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(error => error.Description));
                    throw new InvalidOperationException($"Failed to seed default Super Admin: {errors}");
                }
            }
            else
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
                await userManager.ResetPasswordAsync(adminUser, token, password);
            }

            adminUser.IsActive = true;
            if (!adminUser.EmailConfirmed)
            {
                adminUser.EmailConfirmed = true;
            }
            await userManager.UpdateAsync(adminUser);

            if (!await userManager.IsInRoleAsync(adminUser, Roles.SuperAdmin.ToString()))
            {
                await userManager.AddToRoleAsync(adminUser, Roles.SuperAdmin.ToString());
            }
        }
    }
}