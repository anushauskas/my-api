using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Service.Services;

namespace CleanArchitecture.Service;
internal static class DependencyInjection
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddScoped<IUser, CurrentUser>()
            .AddScoped<IIdentityService, IdentityService>();

        return services;
    }
}
