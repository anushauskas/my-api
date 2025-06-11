using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.TodoItems.Queries.GetTodoItemsWithPagination;

namespace CleanArchitecture.Application.WeatherForecasts.Queries.GetWeatherForecasts;

public record GetWeatherForecastsQuery;

public class GetWeatherForecastsQueryHandler : IRequestHandler<GetWeatherForecastsQuery, PaginatedList<TodoItemBriefDto>>
{
    private readonly ITodoItemsApiClient _todoItemsApiClient;

    public GetWeatherForecastsQueryHandler(ITodoItemsApiClient todoItemsApiClient)
    {
        _todoItemsApiClient = todoItemsApiClient;
    }

    public async Task<PaginatedList<TodoItemBriefDto>> Handle(GetWeatherForecastsQuery request, CancellationToken cancellationToken)
    {
        return await _todoItemsApiClient.GetTodoItemsWithPaginationAsync(1, default, default);
    }
}
