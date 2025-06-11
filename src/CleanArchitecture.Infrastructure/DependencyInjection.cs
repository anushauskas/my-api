using System.Diagnostics.Metrics;
using System.Reflection;
using System.Security.Claims;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Domain.Constants;
using CleanArchitecture.Infrastructure.Clients;
using CleanArchitecture.Infrastructure.Configuration;
using CleanArchitecture.Infrastructure.Configuration.Options;
using CleanArchitecture.Infrastructure.Data;
using CleanArchitecture.Infrastructure.Data.Interceptors;
using CleanArchitecture.Infrastructure.HttpHandlers;
using CleanArchitecture.Infrastructure.Telemetry;
using Duende.AccessTokenManagement;
using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;

public static class DependencyInjection
{
    private const string ApOAuth2TokenClientName = "ApOAuth2TokenClient";

    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDatabase(configuration);

        services.AddSingleton<CleanArchitecture.Infrastructure.TimeProvider>();

        services.AddAuthentication(configuration);

        services.AddAuthorization();

        services.AddTelemetry(configuration);

        services
            .AddApiClientInfrastructure()
            .AddOAuth2ApiClientAuthentication(configuration)
            .AddOAuth2ApiClient<TodoItemsApiClientOptions, ITodoItemsApiClient, TodoItemsApiClient>(configuration);

        return services;
    }

    private static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        Guard.Against.Null(connectionString, message: "Connection string 'DefaultConnection' not found.");

        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());

            options.UseSqlServer(connectionString).AddAsyncSeeding(sp);
        });

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitialiser>();
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
            })
            ; ;

        return services;
    }

    private static IServiceCollection AddAuthorization(this IServiceCollection services)
    {
        return services
            .AddAuthorization(o => o.AddPolicy(Policies.CanPurge, policy => policy.RequireRole(Roles.Administrator)));
    }

    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IInstrumentationService, InstrumentationService>();

        var openTelemetryOptions = configuration.GetSection("OpenTelemetry").Get<OpenTelemetryOptions>()!;

        var version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Undefined";
        var serviceInstanceId = Environment.MachineName;

        var appResourceBuilder = ResourceBuilder
            .CreateDefault()
            .AddService(
                openTelemetryOptions.ServiceName.ToLowerInvariant(),
                openTelemetryOptions.ServiceNamespace.ToLowerInvariant(),
                version,
                serviceInstanceId: serviceInstanceId)
            .AddAttributes(new KeyValuePair<string, object>[]
            {
                new("deployment.environment", environment)
            });

        services.AddLogging(config =>
        {
            config
                .AddOpenTelemetry(otBuilder =>
                {
                    otBuilder.IncludeFormattedMessage = true;
                    otBuilder.IncludeScopes = true;
                    otBuilder.ParseStateValues = true;
                    otBuilder
                        .SetResourceBuilder(appResourceBuilder)
                        .AddOtlpExporter(opt =>
                        {
                            opt.Endpoint = new Uri(openTelemetryOptions.Endpoint, "v1/logs");
                            opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                        });
                });
        });

        services.AddOpenTelemetry()
            .WithMetrics(metricProviderBuilder =>
            {
                var emptyBuckets = new ExplicitBucketHistogramConfiguration { Boundaries = new double[0] };

                metricProviderBuilder
                    .AddOtlpExporter((opt, readerConfig) =>
                    {
                        opt.Endpoint = new Uri(openTelemetryOptions.Endpoint, "v1/metrics");
                        opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                        opt.ExportProcessorType = ExportProcessorType.Batch;
                        readerConfig.PeriodicExportingMetricReaderOptions = new PeriodicExportingMetricReaderOptions
                        {
                            ExportIntervalMilliseconds = (int?)openTelemetryOptions.MetricsPushInterval.TotalMilliseconds,
                        };
                    })
                    .SetResourceBuilder(appResourceBuilder)
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddMeter(InstrumentationService.MeterName)
                    .AddView(instrument =>
                    {
                        return instrument.Name switch
                        {
                            // discard some metrics
                            "http.server.active_requests" => MetricStreamConfiguration.Drop,
                            "http.client.active_requests" => MetricStreamConfiguration.Drop,
                            "http.client.open_connections" => MetricStreamConfiguration.Drop,
                            "http.client.connection.duration" => MetricStreamConfiguration.Drop,
                            "http.client.request.time_in_queue" => MetricStreamConfiguration.Drop,
                            // discard histograms buckets
                            _ => instrument is Histogram<double> ? emptyBuckets : null
                        };
                    });
            })
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .AddSource(InstrumentationService.ActivitySourceName)
                    .AddHttpClientInstrumentation()
                    .AddAspNetCoreInstrumentation()
                    .AddSqlClientInstrumentation()
                    .AddOtlpExporter(opt =>
                    {
                        opt.Endpoint = new Uri(openTelemetryOptions.Endpoint, "v1/traces");
                        opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                    })
                    .SetResourceBuilder(appResourceBuilder);
            })
            ;

        return services;
    }

    private static IAsyncPolicy<HttpResponseMessage> CreateTransientHttpErrorPolicy(PolicyBuilder<HttpResponseMessage> policyBuilder) => policyBuilder
        .WaitAndRetryAsync(new[]
        {
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(60)
        });

    public static IServiceCollection AddApiClientInfrastructure(this IServiceCollection services) => services
        .AddTransient<HttpErrorLoggingHandler>();

    private static IHttpClientBuilder AddCommonHttpClientServices(this IHttpClientBuilder builder) => builder
        .AddTransientHttpErrorPolicy(CreateTransientHttpErrorPolicy)
        .AddHttpMessageHandler<HttpErrorLoggingHandler>();

    private static IServiceCollection AddOAuth2ApiClientAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var tokenClientOptions = configuration.GetSection("OAuth2TokenClient").Get<OAuth2TokenClientOptions>()
                ?? throw new Exception("OAuth2TokenClient configuration is missing");

        services.AddHttpClient(ClientCredentialsTokenManagementDefaults.BackChannelHttpClientName)
            .AddCommonHttpClientServices();

        services
            .AddDistributedMemoryCache()
            .AddClientCredentialsTokenManagement()
            .AddClient(ApOAuth2TokenClientName, client =>
            {
                client.TokenEndpoint = tokenClientOptions.OAuth2TokenUrl;
                client.ClientId = tokenClientOptions.OAuth2ClientId;
                client.ClientSecret = tokenClientOptions.OAuth2ClientSecret;
                client.Parameters.Add("grant_type", "client_credentials");
                client.ClientCredentialStyle = ClientCredentialStyle.PostBody;
            });

        return services;
    }

    private static IServiceCollection AddOAuth2ApiClient<TOptions, TApiClientInterface, TApiClient>(this IServiceCollection services, IConfiguration configuration)
        where TOptions : class, IHttpClientOptionsSection, new()
        where TApiClientInterface : class
        where TApiClient : class, TApiClientInterface
    {
        services.AddOptions<TOptions>(configuration);

        services
            .AddHttpClient<TApiClientInterface, TApiClient>((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<TOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl + (options.BaseUrl.EndsWith('/') ? "" : "/"));
                client.Timeout = options.Timeout;
            })
            .AddClientCredentialsTokenHandler(ApOAuth2TokenClientName)
            .AddCommonHttpClientServices();

        return services;
    }
}
