using System.Collections;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using ShiftOne.Application.Services.User;
using ShiftOne.Core.Entities.Identity;
using ShiftOne.Core.Entities.Identity.Base;
using ShiftOne.Core.Interfaces.Infrastructure.Providers;
using ShiftOne.Core.Interfaces.Infrastructure.Repositories;
using ShiftOne.Shared.Constants;

namespace ShiftOne.Tests.Services.User;

internal sealed class UserServiceTestContext
{
    public List<ApplicationUser> Users { get; } = new();
    public List<Customer> Customers { get; } = new();
    public List<Admin> Admins { get; } = new();
    public List<RefreshToken> RefreshTokens { get; } = new();
    public List<ApplicationRole> RolesStore { get; } = new();
    public List<ApplicationRolePermission> RolePermissions { get; } = new();
    public Dictionary<Guid, HashSet<string>> UserRoles { get; } = new();
    public Dictionary<Guid, string> Passwords { get; } = new();
    public HashSet<Guid> LockedUsers { get; } = new();

    public Mock<UserManager<ApplicationUser>> UserManager { get; }
    public Mock<IJwtService> JwtService { get; } = new();
    public Mock<IVerificationService> VerificationService { get; } = new();
    public Mock<IFileService> FileService { get; } = new();
    public TestCurrentUserService CurrentUserService { get; } = new();
    public TestUnitOfWork UnitOfWork { get; }
    public UserService Service { get; }

    public IdentityResult CreateResult { get; set; } = IdentityResult.Success;
    public IdentityResult UpdateResult { get; set; } = IdentityResult.Success;
    public IdentityResult ResetPasswordResult { get; set; } = IdentityResult.Success;
    public string NextResetToken { get; set; } = "reset-token";
    public Queue<string> RefreshTokenQueue { get; } = new(new[] { "refresh-1", "refresh-2", "refresh-3" });
    public int CompleteCalls => UnitOfWork.CompleteCalls;

    public UserServiceTestContext()
    {
        UnitOfWork = new TestUnitOfWork(this);
        UserManager = CreateUserManagerMock();

        JwtService.Setup(service => service.HashRefreshToken(It.IsAny<string>()))
            .Returns((string token) => $"hash:{token}");
        JwtService.Setup(service => service.GenerateJwtToken(It.IsAny<ApplicationUser>(), It.IsAny<UserManager<ApplicationUser>>()))
            .ReturnsAsync((ApplicationUser user, UserManager<ApplicationUser> _) => $"access:{user.Id}");
        JwtService.Setup(service => service.GenerateRefreshToken())
            .ReturnsAsync(() => RefreshTokenQueue.Count > 0 ? RefreshTokenQueue.Dequeue() : Guid.NewGuid().ToString("N"));
        JwtService.Setup(service => service.SaveRefreshToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((Guid userId, string token, string ip) =>
            {
                RefreshTokens.Add(new RefreshToken
                {
                    Id = Guid.NewGuid(),
                    ApplicationUserId = userId,
                    TokenHash = $"hash:{token}",
                    CreatedByIp = ip,
                    CreatedOn = DateTime.UtcNow,
                    ExpiryDate = DateTime.UtcNow.AddDays(7)
                });
                return true;
            });

        FileService.Setup(service => service.GetFileUrlAsync(It.IsAny<string?>()))
            .ReturnsAsync((string? path) => string.IsNullOrWhiteSpace(path) ? null : $"https://files.test/{path}");
        FileService.Setup(service => service.UploadImageAsync(It.IsAny<FilePathType>(), It.IsAny<Guid>(), It.IsAny<IFormFile>()))
            .ReturnsAsync((FilePathType _, Guid ownerId, IFormFile file) => $"/uploads/UserProfiles/{ownerId}/{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}");
        FileService.Setup(service => service.UploadFileAsync(It.IsAny<FilePathType>(), It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<byte[]>()))
            .ReturnsAsync((FilePathType _, Guid ownerId, string fileName, byte[] _) => $"uploads/{ownerId}/{fileName}");
        FileService.Setup(service => service.DeleteFileAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        VerificationService.Setup(service => service.VerifyCodeAsync(It.IsAny<Guid>(), It.IsAny<string>()))
            .ReturnsAsync(false);

        Service = new UserService(
            UserManager.Object,
            JwtService.Object,
            VerificationService.Object,
            UnitOfWork,
            CurrentUserService,
            FileService.Object);
    }

    public Customer AddCustomer(
        string? email = "user@test.com",
        string? phone = "+201000000000",
        bool isActive = true,
        bool emailConfirmed = true,
        bool phoneConfirmed = true,
        string password = "Password1")
    {
        var user = new Customer
        {
            Id = Guid.NewGuid(),
            UserName = Guid.NewGuid().ToString(),
            FirstName = "First",
            LastName = "Last",
            Email = email,
            PhoneNumber = phone,
            EmailConfirmed = emailConfirmed,
            PhoneNumberConfirmed = phoneConfirmed,
            IsActive = isActive,
            CreatedOn = DateTime.UtcNow
        };

        Users.Add(user);
        Customers.Add(user);
        Passwords[user.Id] = password;
        AddRole(user, Roles.Customer.ToString());
        return user;
    }

    public Admin AddAdmin(string email = "admin@test.com", bool isActive = true, string password = "Password1")
    {
        var admin = new Admin
        {
            Id = Guid.NewGuid(),
            UserName = Guid.NewGuid().ToString(),
            FirstName = "Admin",
            LastName = "User",
            Email = email,
            EmailConfirmed = true,
            IsActive = isActive,
            CreatedOn = DateTime.UtcNow
        };

        Users.Add(admin);
        Admins.Add(admin);
        Passwords[admin.Id] = password;
        AddRole(admin, Roles.Admin.ToString());
        AddRole(admin, Roles.SuperAdmin.ToString());
        return admin;
    }

    public void AddRole(ApplicationUser user, string roleName)
    {
        if (!UserRoles.TryGetValue(user.Id, out var roles))
        {
            roles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            UserRoles[user.Id] = roles;
        }

        roles.Add(roleName);
    }

    public void AddActiveRole(string roleName)
    {
        RolesStore.Add(new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant(),
            IsActive = true
        });
    }

    public void AddRolePermission(string roleName, string permissionName, bool roleActive = true)
    {
        var role = new ApplicationRole
        {
            Id = Guid.NewGuid(),
            Name = roleName,
            NormalizedName = roleName.ToUpperInvariant(),
            IsActive = roleActive
        };
        var permission = new ApplicationPermission
        {
            Id = Guid.NewGuid(),
            Name = permissionName
        };

        RolesStore.Add(role);
        RolePermissions.Add(new ApplicationRolePermission
        {
            Id = Guid.NewGuid(),
            Role = role,
            RoleId = role.Id,
            Permission = permission,
            PermissionId = permission.Id
        });
    }

    public RefreshToken AddRefreshToken(ApplicationUser user, string token, bool active = true)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            ApplicationUserId = user.Id,
            ApplicationUser = user,
            TokenHash = $"hash:{token}",
            CreatedOn = DateTime.UtcNow.AddMinutes(-5),
            CreatedByIp = "127.0.0.1",
            ExpiryDate = active ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddDays(-1),
            IsRevoked = !active,
            RevokedOn = active ? null : DateTime.UtcNow.AddMinutes(-1)
        };
        RefreshTokens.Add(refreshToken);
        return refreshToken;
    }

    private Mock<UserManager<ApplicationUser>> CreateUserManagerMock()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        var manager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        manager.SetupGet(m => m.Users).Returns(() => Users.AsAsyncQueryable());
        manager.Setup(m => m.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) => Users.FirstOrDefault(user => string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase)));
        manager.Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync((string id) => Guid.TryParse(id, out var parsed)
                ? Users.FirstOrDefault(user => user.Id == parsed)
                : null);
        manager.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser user, string password) =>
            {
                if (CreateResult.Succeeded && Users.All(existing => existing.Id != user.Id))
                {
                    Users.Add(user);
                    if (user is Customer customer)
                    {
                        Customers.Add(customer);
                    }
                    if (user is Admin admin)
                    {
                        Admins.Add(admin);
                    }
                    Passwords[user.Id] = password;
                }
                return CreateResult;
            });
        manager.Setup(m => m.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser user, string roleName) =>
            {
                AddRole(user, roleName);
                return IdentityResult.Success;
            });
        manager.Setup(m => m.IsInRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser user, string roleName) =>
                UserRoles.TryGetValue(user.Id, out var roles) && roles.Contains(roleName));
        manager.Setup(m => m.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((ApplicationUser user) =>
                UserRoles.TryGetValue(user.Id, out var roles)
                    ? roles.OrderBy(role => role).ToList()
                    : new List<string>());
        manager.Setup(m => m.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser user, string password) =>
                Passwords.TryGetValue(user.Id, out var savedPassword) && savedPassword == password);
        manager.Setup(m => m.AccessFailedAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        manager.Setup(m => m.ResetAccessFailedCountAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        manager.Setup(m => m.IsLockedOutAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync((ApplicationUser user) => LockedUsers.Contains(user.Id));
        manager.Setup(m => m.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(() => UpdateResult);
        manager.Setup(m => m.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(() => NextResetToken);
        manager.Setup(m => m.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser user, string _, string password) =>
            {
                if (ResetPasswordResult.Succeeded)
                {
                    Passwords[user.Id] = password;
                }
                return ResetPasswordResult;
            });
        manager.Setup(m => m.UpdateSecurityStampAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);

        return manager;
    }
}

internal sealed class TestCurrentUserService : ICurrentUserService
{
    public Guid? CurrentUserId { get; set; }
    public Guid? CurrentCompanyId { get; set; }
    public bool IsSuperAdmin { get; set; }
    public bool IsPlatformAdmin { get; set; }
    public bool IsCompanyAdmin { get; set; }
    public bool IsHr { get; set; }
    public string CurrentUserName { get; set; } = "tester";
    public string? CurrentIpAddress { get; set; } = "127.0.0.1";
    public bool? IsActived { get; set; } = true;
    public string BaseUrl { get; set; } = "https://api.test/";
    public string GetBaseUrl(string relativePath) => $"{BaseUrl}{relativePath}";
}

internal sealed class TestUnitOfWork : IUnitOfWork
{
    private readonly UserServiceTestContext _context;
    public int CompleteCalls { get; private set; }

    public TestUnitOfWork(UserServiceTestContext context)
    {
        _context = context;
    }

    public IRepository<T> Repository<T>() where T : class
    {
        if (typeof(T) == typeof(Customer))
            return (IRepository<T>)(object)new TestRepository<Customer>(_context.Customers);
        if (typeof(T) == typeof(Admin))
            return (IRepository<T>)(object)new TestRepository<Admin>(_context.Admins);
        if (typeof(T) == typeof(ApplicationUser))
        {
            var allUsers = new List<ApplicationUser>();
            allUsers.AddRange(_context.Customers);
            allUsers.AddRange(_context.Admins);
            return (IRepository<T>)(object)new TestRepository<ApplicationUser>(allUsers);
        }
        if (typeof(T) == typeof(RefreshToken))
            return (IRepository<T>)(object)new TestRepository<RefreshToken>(_context.RefreshTokens);
        if (typeof(T) == typeof(ApplicationRole))
            return (IRepository<T>)(object)new TestRepository<ApplicationRole>(_context.RolesStore);
        if (typeof(T) == typeof(ApplicationRolePermission))
            return (IRepository<T>)(object)new TestRepository<ApplicationRolePermission>(_context.RolePermissions);

        throw new InvalidOperationException($"No test repository configured for {typeof(T).Name}.");
    }

    public Task<int> CompleteAsync()
    {
        CompleteCalls++;
        return Task.FromResult(1);
    }

    public void Dispose()
    {
    }
}

internal sealed class TestRepository<T> : IRepository<T> where T : class
{
    private readonly List<T> _items;

    public TestRepository(List<T> items)
    {
        _items = items;
    }

    public Task<T?> GetByIdAsync(Guid Id)
    {
        var item = _items.FirstOrDefault(entity =>
        {
            var property = entity.GetType().GetProperty("Id");
            return property?.GetValue(entity) is Guid entityId && entityId == Id;
        });
        return Task.FromResult(item);
    }

    public Task<IEnumerable<T>> GetAllAsync(ISpecification<T>? specification = null)
    {
        IEnumerable<T> query = _items;
        if (specification?.Criteria != null)
        {
            query = query.Where(specification.Criteria.Compile());
        }
        if (specification?.OrderBy != null)
        {
            query = query.OrderBy(specification.OrderBy.Compile());
        }
        if (specification?.OrderByDescending != null)
        {
            query = query.OrderByDescending(specification.OrderByDescending.Compile());
        }
        if (specification?.Skip != null)
        {
            query = query.Skip(specification.Skip.Value);
        }
        if (specification?.Take != null)
        {
            query = query.Take(specification.Take.Value);
        }

        return Task.FromResult(query);
    }

    public Task<int> CountAsync(ISpecification<T>? specification = null)
    {
        IEnumerable<T> query = _items;
        if (specification?.Criteria != null)
        {
            query = query.Where(specification.Criteria.Compile());
        }

        return Task.FromResult(query.Count());
    }
    public Task AddAsync(T entity)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(T entity) => Task.CompletedTask;
    public Task DeleteAsync(T entity)
    {
        _items.Remove(entity);
        return Task.CompletedTask;
    }

    public Task SaveAsync() => Task.CompletedTask;
}

internal static class AsyncQueryableExtensions
{
    public static IQueryable<T> AsAsyncQueryable<T>(this IEnumerable<T> source)
    {
        return new TestAsyncEnumerable<T>(source);
    }
}

internal sealed class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    private readonly IQueryProvider _provider;

    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    {
        _provider = enumerable.AsQueryable().Provider;
    }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    {
        _provider = ((IQueryable<T>)new EnumerableQuery<T>(expression)).Provider;
    }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(_provider);
}

internal sealed class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public T Current => _inner.Current;

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return ValueTask.FromResult(_inner.MoveNext());
    }
}

internal sealed class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    public TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object? Execute(Expression expression)
    {
        return _inner.Execute(expression);
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var resultType = typeof(TResult).GetGenericArguments().FirstOrDefault() ?? typeof(TResult);
        var executionResult = typeof(IQueryProvider)
            .GetMethods()
            .Single(method => method.Name == nameof(IQueryProvider.Execute) &&
                              method.IsGenericMethod &&
                              method.GetParameters().Length == 1)
            .MakeGenericMethod(resultType)
            .Invoke(_inner, new object[] { expression });

        if (typeof(TResult).IsGenericType && typeof(TResult).GetGenericTypeDefinition() == typeof(Task<>))
        {
            return (TResult)typeof(Task)
                .GetMethod(nameof(Task.FromResult))!
                .MakeGenericMethod(resultType)
                .Invoke(null, new[] { executionResult })!;
        }

        return (TResult)executionResult!;
    }
}





