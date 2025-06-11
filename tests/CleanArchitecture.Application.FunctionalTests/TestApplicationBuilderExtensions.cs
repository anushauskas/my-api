using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CleanArchitecture.Application.FunctionalTests;
internal static class TestApplicationBuilderExtensions
{
    public static HostApplicationBuilder CreateApplicationBuilder()
    {
        var environment = "Testing";
#if DEBUG
        //environment = "Development";
#endif

        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings()
        {
            EnvironmentName = environment
        });

        builder.Services
            .AddApplicationServices()
            .AddInfrastructureServices(builder.Configuration);

        return builder;
    }

    public static HostApplicationBuilder WithCustomScoped<TService>(this HostApplicationBuilder builder, TService instance)
        where TService : class
    {
        builder.Services
            .RemoveAll<TService>()
            .AddScoped(sp => instance);

        return builder;
    }
}
