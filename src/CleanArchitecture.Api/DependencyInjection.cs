using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Infrastructure.Configuration;
using CleanArchitecture.Infrastructure.Configuration.Options;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using NSwag;
using NSwag.Generation.Processors.Security;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddHttpContextAccessor();

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

        services.AddControllers();

        // Customise default API behaviour
        services.Configure<ApiBehaviorOptions>(options =>
            options.SuppressModelStateInvalidFilter = true);

        services.AddApiExplorer(configuration);

        return services;
    }

    public static IServiceCollection AddApiExplorer(this IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddOptions<SwaggerConfigurationOptions>(configuration)
            .AddEndpointsApiExplorer()
            .AddOpenApiDocument((configure, sp) =>
            {
                var options = sp.GetRequiredService<IOptions<SwaggerConfigurationOptions>>().Value;
                configure.Title = "CleanArchitecture API";

                // Add JWT
                configure.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
                {
                    Type = OpenApiSecuritySchemeType.OAuth2,
                    Name = "Authorization",
                    In = OpenApiSecurityApiKeyLocation.Header,
                    Description = "Type into the textbox: Bearer {your JWT token}.",
                    Flows = new()
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            TokenUrl = options.TokenUrl,
                            AuthorizationUrl = options.AuthorizationUrl,
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "Open Id." }
                            },
                        },
                    }
                });



                configure.OperationProcessors.Add(new AspNetCoreOperationSecurityScopeProcessor("JWT"));
            });
    }
}
