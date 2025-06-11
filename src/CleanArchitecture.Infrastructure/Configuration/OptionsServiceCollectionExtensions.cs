using CleanArchitecture.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure.Configuration;
public static class OptionsServiceCollectionExtensions
{
    public static IServiceCollection AddOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration
    )
        where TOptions : class, IOptionsSection, new()
    {
        services
            .AddOptions<TOptions>()
            .Bind(configuration.GetSection(new TOptions().SectionName))
            .Validate(o => o.Validate());

        return services;
    }
}
