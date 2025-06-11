using System.Reflection;
using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.TodoItems.Commands.CreateTodoItem;
using CleanArchitecture.Application.TodoItems.Commands.DeleteTodoItem;
using CleanArchitecture.Application.TodoItems.Commands.UpdateTodoItem;
using CleanArchitecture.Application.TodoItems.Commands.UpdateTodoItemDetail;
using CleanArchitecture.Application.TodoItems.Queries.GetTodoItemsWithPagination;
using CleanArchitecture.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Application.TodoLists.Commands.DeleteTodoList;
using CleanArchitecture.Application.TodoLists.Commands.PurgeTodoLists;
using CleanArchitecture.Application.TodoLists.Commands.UpdateTodoList;
using CleanArchitecture.Application.TodoLists.Queries.GetTodos;
using CleanArchitecture.Application.WeatherForecasts.Queries.GetWeatherForecasts;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services
            .AddScoped(typeof(RequestHandler<>))
            .AddScoped(typeof(RequestHandler<,>))
            .AddScoped(typeof(IPipelineBehavior<>), typeof(UnhandledExceptionBehaviour<>))
            .AddScoped(typeof(IPipelineBehavior<>), typeof(AuthorizationBehaviour<>))
            .AddScoped(typeof(IPipelineBehavior<>), typeof(ValidationBehaviour<>))
            .AddScoped(typeof(IPipelineBehavior<>), typeof(PerformanceBehaviour<>));

        services
            .AddScoped<IRequestHandler<GetWeatherForecastsQuery, PaginatedList<TodoItemBriefDto>>, GetWeatherForecastsQueryHandler>()
            .AddScoped<IRequestHandler<PurgeTodoListsCommand>, PurgeTodoListsCommandHandler>()
            .AddScoped<IRequestHandler<DeleteTodoListCommand>, DeleteTodoListCommandHandler>()
            .AddScoped<IRequestHandler<UpdateTodoListCommand>, UpdateTodoListCommandHandler>()
            .AddScoped<IRequestHandler<CreateTodoListCommand, int>, CreateTodoListCommandHandler>()
            .AddScoped<IRequestHandler<GetTodosQuery, TodosVm>, GetTodosQueryHandler>()
            .AddScoped<IRequestHandler<GetTodoItemsWithPaginationQuery, PaginatedList<TodoItemBriefDto>>, GetTodoItemsWithPaginationQueryHandler>()
            .AddScoped<IRequestHandler<CreateTodoItemCommand, int>, CreateTodoItemCommandHandler>()
            .AddScoped<IRequestHandler<UpdateTodoItemCommand>, UpdateTodoItemCommandHandler>()
            .AddScoped<IRequestHandler<DeleteTodoItemCommand>, DeleteTodoItemCommandHandler>()
            .AddScoped<IRequestHandler<UpdateTodoItemDetailCommand>, UpdateTodoItemDetailCommandHandler>();

        return services;
    }
}
