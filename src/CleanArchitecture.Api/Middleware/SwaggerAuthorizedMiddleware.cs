using CleanArchitecture.Infrastructure.Configuration.Options;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;

namespace CleanArchitecture.Api.Middleware;

public class SwaggerAuthorizedMiddleware
{
    private readonly RequestDelegate _next;
    private readonly SwaggerConfigurationOptions _config;

    public SwaggerAuthorizedMiddleware(RequestDelegate next, IOptions<SwaggerConfigurationOptions> config)
    {
        _next = next;
        _config = config.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger")
            && !IsLocalRequest(context)
            && !string.IsNullOrEmpty(_config.User))
        {
            var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Basic"))
            {
                // Get the encoded username and password
                var encodedUsernamePassword =
                    authHeader.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries)[1]?.Trim() ?? string.Empty;

                // Decode from Base64 to string
                var decodedUsernamePassword =
                    Encoding.UTF8.GetString(Convert.FromBase64String(encodedUsernamePassword));

                // Split username and password
                var username = decodedUsernamePassword.Split(':', 2)[0];
                var password = decodedUsernamePassword.Split(':', 2)[1];

                // Check if login is correct
                if (IsAuthorized(username, password))
                {
                    await _next.Invoke(context);
                    return;
                }
            }

            // Return authentication type (causes browser to show login dialog)
            context.Response.Headers["WWW-Authenticate"] = "Basic";

            // Return unauthorized
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
        else
        {
            await _next.Invoke(context);
        }
    }

    public bool IsAuthorized(string username, string password)
    {
        // Check that username and password are correct
        return username.Equals(_config.User, StringComparison.InvariantCultureIgnoreCase)
               && password.Equals(_config.Password);
    }

    private static bool IsLocalRequest(HttpContext context)
    {
        return
            context.Connection.RemoteIpAddress == null && context.Connection.LocalIpAddress == null ||
            context.Connection.RemoteIpAddress != null &&
             (context.Connection.RemoteIpAddress.Equals(context.Connection.LocalIpAddress) ||
              IPAddress.IsLoopback(context.Connection.RemoteIpAddress));
    }
}
