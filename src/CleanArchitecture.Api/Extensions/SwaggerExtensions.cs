using CleanArchitecture.Api.Middleware;
using CleanArchitecture.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;

namespace CleanArchitecture.Api.Extensions;

public static class SwaggerExtensions
{
    public static IApplicationBuilder UseSwaggerWithUi(this IApplicationBuilder builder)
    {
        var swaggerConfig = builder.ApplicationServices.GetService<IOptions<SwaggerConfigurationOptions>>()?.Value;

        if (swaggerConfig != null && swaggerConfig.Enabled)
        {
            builder
                .UseMiddleware<SwaggerAuthorizedMiddleware>()
                .UseOpenApi();

            if (swaggerConfig.UiEnabled)
            {
                builder.UseSwaggerUi(configure =>
                {
                    configure.OAuth2Client = new NSwag.AspNetCore.OAuth2ClientSettings()
                    {
                        ClientId = swaggerConfig.OAuthClientId,
                        ClientSecret = string.Empty,
                        AppName = "My Test API",
                        UsePkceWithAuthorizationCodeGrant = true
                    };
                });
            }
        }

        return builder;
    }
}
