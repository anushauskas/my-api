using System.Security.Claims;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Infrastructure.Configuration;
using CleanArchitecture.Infrastructure.Configuration.Options;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Web.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddAuthentication(configuration);
        services.AddAuthorizationWithPolicies();

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

    private static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var tokenSettings = configuration.GetSection("TokenManagement").Get<TokenManagementOptions>()!;
        services
            .AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = "DynamicScheme";
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            // Accept tests: 
            .AddJwtBearer("Testing", o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(tokenSettings.SymmetricKey)),
                    ValidIssuer = tokenSettings.Issuer,
                    ValidAudience = tokenSettings.Audience,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.FromSeconds(5),
                    NameClaimType = ClaimTypes.NameIdentifier,
                    RoleClaimType = "scopes"
                };
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
            {
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = tokenSettings.Issuer,
                    ValidAudience = tokenSettings.Audience,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.FromSeconds(5),
                    // our internal athentification service specifics: 
                    NameClaimType = ClaimTypes.NameIdentifier,
                    // Need to check what defines user permissions in Okta, but for now we use scopes:
                    RoleClaimType = "http://schemas.microsoft.com/identity/claims/scope"
                };
                o.Authority = tokenSettings.Authority;
                o.RequireHttpsMetadata = true;
            })
            .AddPolicyScheme("DynamicScheme", "OAuth2 Token Client", options =>
            {
                options.ForwardDefaultSelector = context =>
                {
                    // If the request has an Authorization header, use the Testing scheme
                    if (context.Request.Headers.Authorization.Any(x => !string.IsNullOrWhiteSpace(x))
                        && context.Request.Headers.Authorization.First()!.StartsWith("Testing"))
                    {
                        context.Request.Headers.Authorization = context.Request.Headers.Authorization.First()!.Replace("Testing", "Bearer").Trim();
                        return "Testing";
                    }
                    // Otherwise, use the default JwtBearer scheme
                    return JwtBearerDefaults.AuthenticationScheme;
                };
            });

        return services;
    }

    private static IServiceCollection AddAuthorizationWithPolicies(this IServiceCollection services)
    {
        return services
            .AddAuthorization(o => o.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));
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
