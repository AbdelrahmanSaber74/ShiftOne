using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ShiftOne.Core.Entities.Attendance;
using ShiftOne.Core.Entities.Branches;
using ShiftOne.Core.Entities.Companies;
using ShiftOne.Core.Entities.Contracts;
using ShiftOne.Core.Entities.Holidays;
using ShiftOne.Core.Entities.Identity;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Entities.Logging;
using ShiftOne.Core.Entities.Subscriptions;
using ShiftOne.Core.Entities.WorkSchedules;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;

namespace ShiftOne.Infrastructure.Persistence
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        private readonly ICurrentUserService _currentUserService;

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<ApplicationRole> ApplicationRoles { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<ApplicationPermission> Permissions { get; set; }
        public DbSet<ApplicationRolePermission> RolePermissions { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<CompanySubscription> CompanySubscriptions { get; set; }
        public DbSet<Branch> Branches { get; set; }
        public DbSet<EmployeeDevice> EmployeeDevices { get; set; }
        public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
        public DbSet<CompanyHoliday> CompanyHolidays { get; set; }
        public DbSet<WorkSchedule> WorkSchedules { get; set; }
        public DbSet<WorkScheduleDay> WorkScheduleDays { get; set; }
        public DbSet<LogEntry> Logs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
            ICurrentUserService currentUserService)
            : base(options)
        {
            _currentUserService = currentUserService;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<LogEntry>(entity =>
            {
                entity.ToTable("Logs");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Timestamp).HasColumnType("datetime2");
                entity.Property(x => x.Level).HasMaxLength(32);
                entity.Property(x => x.Message).IsRequired();
                entity.Property(x => x.CorrelationId).HasMaxLength(64);
                entity.Property(x => x.RequestMethod).HasMaxLength(16);
                entity.Property(x => x.RequestPath).HasMaxLength(2048);
                entity.Property(x => x.ClientIp).HasMaxLength(64);
                entity.Property(x => x.ApplicationName).HasMaxLength(128);
                entity.Property(x => x.EnvironmentName).HasMaxLength(64);
                entity.Property(x => x.MachineName).HasMaxLength(128);
                entity.HasIndex(x => x.Timestamp);
                entity.HasIndex(x => x.Level);
                entity.HasIndex(x => x.CorrelationId);
                entity.HasIndex(x => x.RequestPath);
            });
            builder.Entity<ApplicationUser>().ToTable("Users");
            builder.Entity<ApplicationRole>().ToTable("ApplicationRoles");
            builder.Entity<ApplicationUser>()
                .HasDiscriminator<string>("UserKind")
                .HasValue<ApplicationUser>("User")
                .HasValue<Admin>("Admin")
                .HasValue<Customer>("Customer");

            builder.Entity<ApplicationRolePermission>(entity =>
            {
                entity.HasKey(x => x.Id);

                entity.HasOne(x => x.Role)
                      .WithMany(r => r.RolePermissions)
                      .HasForeignKey(x => x.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Permission)
                      .WithMany(p => p.RolePermissions)
                      .HasForeignKey(x => x.PermissionId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new { x.RoleId, x.PermissionId }).IsUnique();
            });

            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.PhoneNumber)
                .IsUnique()
                .HasFilter("[PhoneNumber] IS NOT NULL");
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.CompanyId);
            builder.Entity<ApplicationUser>()
                .HasIndex(u => u.BranchId);
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.RefreshTokens)
                .WithOne(r => r.ApplicationUser)
                .HasForeignKey(c => c.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.Restrict);
            builder.Entity<ApplicationUser>()
                .HasOne(u => u.Branch)
                .WithMany(b => b.Users)
                .HasForeignKey(u => u.BranchId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<RefreshToken>(entity =>
            {
                entity.HasIndex(x => x.TokenHash).IsUnique();
                entity.HasIndex(x => x.ApplicationUserId);
            });

            builder.Entity<Company>(entity =>
            {
                entity.HasIndex(x => x.Code).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(150);
                entity.Property(x => x.Code).HasMaxLength(50);
            });

            builder.Entity<SubscriptionPlan>(entity =>
            {
                entity.Property(x => x.Name).HasMaxLength(100);
                entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            });

            builder.Entity<CompanySubscription>(entity =>
            {
                entity.HasIndex(x => new { x.CompanyId, x.IsActive });
                entity.HasOne(x => x.Company)
                    .WithMany(x => x.Subscriptions)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Plan)
                    .WithMany(x => x.CompanySubscriptions)
                    .HasForeignKey(x => x.PlanId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Branch>(entity =>
            {
                entity.HasIndex(x => new { x.CompanyId, x.Code }).IsUnique();
                entity.HasIndex(x => new { x.CompanyId, x.IsMainBranch })
                    .IsUnique()
                    .HasFilter("[IsMainBranch] = 1");
                entity.Property(x => x.Latitude).HasColumnType("decimal(9,6)");
                entity.Property(x => x.Longitude).HasColumnType("decimal(9,6)");
                entity.HasOne(x => x.Company)
                    .WithMany(x => x.Branches)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            builder.Entity<CompanyHoliday>(entity =>
            {
                entity.HasIndex(x => new { x.CompanyId, x.Date, x.Name }).IsUnique();
                entity.HasIndex(x => new { x.Date, x.IsActive });
                entity.Property(x => x.Name).HasMaxLength(150);
                entity.Property(x => x.Notes).HasMaxLength(500);
                entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
            builder.Entity<WorkSchedule>(entity =>
            {
                entity.HasIndex(x => new { x.CompanyId, x.Name }).IsUnique();
                entity.HasIndex(x => new { x.CompanyId, x.IsDefault })
                    .IsUnique()
                    .HasFilter("[IsDefault] = 1 AND [IsActive] = 1");
                entity.Property(x => x.Name).HasMaxLength(120);
                entity.Property(x => x.Description).HasMaxLength(500);
                entity.Property(x => x.TimeZoneId).HasMaxLength(100);
                entity.HasOne(x => x.Company)
                    .WithMany(x => x.WorkSchedules)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<WorkScheduleDay>(entity =>
            {
                entity.HasIndex(x => new { x.WorkScheduleId, x.DayOfWeek }).IsUnique();
                entity.HasOne(x => x.WorkSchedule)
                    .WithMany(x => x.Days)
                    .HasForeignKey(x => x.WorkScheduleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            builder.Entity<EmployeeDevice>(entity =>
            {
                entity.HasIndex(x => new { x.EmployeeId, x.IsActive })
                    .IsUnique()
                    .HasFilter("[IsActive] = 1");
                entity.HasIndex(x => x.DeviceId);
                entity.HasOne(x => x.Employee)
                    .WithMany(x => x.EmployeeDevices)
                    .HasForeignKey(x => x.EmployeeId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AttendanceRecord>(entity =>
            {
                entity.HasIndex(x => new { x.CompanyId, x.EmployeeId, x.AttendanceDate }).IsUnique();
                entity.Property(x => x.CheckInLatitude).HasColumnType("decimal(9,6)");
                entity.Property(x => x.CheckInLongitude).HasColumnType("decimal(9,6)");
                entity.Property(x => x.CheckOutLatitude).HasColumnType("decimal(9,6)");
                entity.Property(x => x.CheckOutLongitude).HasColumnType("decimal(9,6)");
                entity.HasOne(x => x.Company).WithMany().HasForeignKey(x => x.CompanyId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Employee).WithMany().HasForeignKey(x => x.EmployeeId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.WorkSchedule).WithMany().HasForeignKey(x => x.WorkScheduleId).OnDelete(DeleteBehavior.Restrict);
                entity.Property(x => x.FinalStatus).HasConversion<string>().HasMaxLength(40);
                entity.Property(x => x.WorkScheduleName).HasMaxLength(120);
            });
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker.Entries<IAuditableEntity>();

            foreach (var entry in entries)
            {
                var now = DateTime.UtcNow;
                var userId = _currentUserService.CurrentUserId ?? Guid.Empty;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedOn = now;
                        entry.Entity.CreatedBy = userId;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedOn = now;
                        entry.Entity.UpdatedBy = userId;
                        break;
                }
            }

            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
