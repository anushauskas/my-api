using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Application.FunctionalTests;

[SetUpFixture]
public partial class Testing
{
    private static ITestDatabase _database = null!;
    private static Microsoft.Extensions.Hosting.IHost _host;
    private static IServiceScopeFactory _scopeFactory = null!;
    private static string? _userId;
    private static string? _userName;
    private static string[]? _roles;

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        var identyService = new Mock<IIdentityService>();
        identyService
            .Setup(x => x.IsInRoleAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string user, string role) => GetUserRoles().Any(r => r.Equals(role)));

        identyService
            .Setup(x => x.AuthorizeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync((string userId, string policyName) =>
                GetUserRoles().Any(r => r.Equals(Roles.Administrator))
                && policyName.Equals(Policies.CanPurge)
            );

        var user = new Mock<IUser>();
        user.Setup(u => u.Id).Returns(GetUserId);

        var hostBuilder = TestApplicationBuilderExtensions
            .CreateApplicationBuilder()
            .WithCustomScoped(identyService.Object)
            .WithCustomScoped(user.Object);

        _host = hostBuilder.Build();

        _scopeFactory = _host.Services.GetRequiredService<IServiceScopeFactory>();

        _database = await TestDatabaseFactory.CreateAsync(hostBuilder.Configuration);
    }

    public static async Task<TResponse> SendAsync<TRequest, TResponse>(TRequest request) where TRequest : notnull
    {
        using var scope = _scopeFactory.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<RequestHandler<TRequest, TResponse>>();

        return await handler.Handle(request, CancellationToken.None);
    }

    public static async Task SendAsync<TRequest>(TRequest request) where TRequest : notnull
    {
        using var scope = _scopeFactory.CreateScope();

        var handler = scope.ServiceProvider.GetRequiredService<RequestHandler<TRequest>>();

        await handler.Handle(request, CancellationToken.None);
    }

    public static string? GetUserId()
    {
        return _userId;
    }

    public static string? GetUserName()
    {
        return _userName;
    }

    public static string[] GetUserRoles()
    {
        return _roles ?? Array.Empty<string>();
    }

    public static string[] GetPolicies() 
    {
        return GetUserRoles();
    }

    public static async Task<string> RunAsDefaultUserAsync()
    {
        return await RunAsUserAsync("test@local", Array.Empty<string>());
    }

    public static async Task<string> RunAsAdministratorAsync()
    {
        return await RunAsUserAsync("administrator@local", new[] { Roles.Administrator });
    }

    public static Task<string> RunAsUserAsync(string userName, string[] roles)
    {
        _roles = roles;
        _userId = Guid.NewGuid().ToString();
        _userName = userName;

        return Task.FromResult(_userId);
    }

    public static async Task ResetState()
    {
        try
        {
            await _database.ResetAsync();
        }
        catch (Exception) 
        {
        }

        _userId = null;
        _roles = null;
        _userName = null;
    }

    public static async Task<TEntity?> FindAsync<TEntity>(params object[] keyValues)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.FindAsync<TEntity>(keyValues);
    }

    public static async Task AddAsync<TEntity>(TEntity entity)
        where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        context.Add(entity);

        await context.SaveChangesAsync();
    }

    public static async Task<int> CountAsync<TEntity>() where TEntity : class
    {
        using var scope = _scopeFactory.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Set<TEntity>().CountAsync();
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        _host.Dispose();
        await _database.DisposeAsync();
    }
}
