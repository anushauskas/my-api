using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.TodoItems.Queries.GetTodoItemsWithPagination;
using CleanArchitecture.Application.WeatherForecasts.Queries.GetWeatherForecasts;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebUI.Controllers;

public class WeatherForecastController : ApiControllerBase
{
    [HttpGet]
    public async Task<PaginatedList<TodoItemBriefDto>> Get(
        RequestHandler<GetWeatherForecastsQuery, PaginatedList<TodoItemBriefDto>> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(new GetWeatherForecastsQuery(), cancellationToken);
    }
}
