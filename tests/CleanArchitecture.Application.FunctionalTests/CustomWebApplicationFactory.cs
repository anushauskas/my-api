using System.Data.Common;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CleanArchitecture.Application.FunctionalTests;

using static Testing;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly DbConnection _connection;
    private readonly string _connectionString;

    public CustomWebApplicationFactory(DbConnection connection, string connectionString)
    {
        _connection = connection;
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
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

        builder.ConfigureTestServices(services =>
        {
            services
                .RemoveAll<IUser>()
                .AddTransient(provider => Mock.Of<IUser>(s => s.Id == GetUserId()))
                .RemoveAll<IIdentityService>()
                .AddTransient(provider => identyService.Object);

            services
                .RemoveAll<DbContextOptions<ApplicationDbContext>>()
                .AddDbContext<ApplicationDbContext>((sp, options) =>
                {
                    options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                    options.UseSqlServer(_connection);
                });
        });
    }
}
