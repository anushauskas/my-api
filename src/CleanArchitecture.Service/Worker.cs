using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.TodoLists.Queries.GetTodos;
using Microsoft.AspNetCore.Hosting.Server;

namespace CleanArchitecture.Service;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHostApplicationLifetime _hostApplication;

    public Worker(ILogger<Worker> logger,
        IServiceScopeFactory serviceScopeFactory,
        IHostApplicationLifetime hostApplication)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
        _hostApplication = hostApplication;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var handler = scope.ServiceProvider.GetRequiredService<RequestHandler<GetTodosQuery, TodosVm>>();
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await handler.Handle(new GetTodosQuery(), stoppingToken);
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
        finally
        {
            _hostApplication.StopApplication();
        }
    }
}
