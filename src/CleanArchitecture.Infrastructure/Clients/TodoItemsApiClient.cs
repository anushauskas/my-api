using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.TodoItems.Queries.GetTodoItemsWithPagination;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace CleanArchitecture.Infrastructure.Clients;
internal class TodoItemsApiClient : ITodoItemsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonSerializerOptions;
    public TodoItemsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        _jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    }
    public async Task<PaginatedList<TodoItemBriefDto>> GetTodoItemsWithPaginationAsync(int listId, int? pageNumber, int? pageSize)
    {
        var query = $"api/TodoItems?ListId={listId}"
            + (pageNumber.HasValue ? $"&PageNumber={pageNumber}" : string.Empty)
            + (pageSize.HasValue ? $"&PageSize={pageSize}" : string.Empty);
        var response = await _httpClient.GetAsync(query);

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Couldn't get todo items for list id '{listId}'. (status code: {response.StatusCode}).");
        }

        return (await response.Content.ReadFromJsonAsync<PaginatedList<TodoItemBriefDto>>(_jsonSerializerOptions))!;
    }
}
