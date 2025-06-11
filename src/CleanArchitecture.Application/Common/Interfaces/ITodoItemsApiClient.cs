using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.TodoItems.Queries.GetTodoItemsWithPagination;

namespace CleanArchitecture.Application.Common.Interfaces;
public interface ITodoItemsApiClient
{
    Task<PaginatedList<TodoItemBriefDto>> GetTodoItemsWithPaginationAsync(int listId, int? pageNumber, int? pageSize);
}
