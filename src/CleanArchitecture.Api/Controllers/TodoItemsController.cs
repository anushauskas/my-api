using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.TodoItems.Commands.CreateTodoItem;
using CleanArchitecture.Application.TodoItems.Commands.DeleteTodoItem;
using CleanArchitecture.Application.TodoItems.Commands.UpdateTodoItem;
using CleanArchitecture.Application.TodoItems.Commands.UpdateTodoItemDetail;
using CleanArchitecture.Application.TodoItems.Queries.GetTodoItemsWithPagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CleanArchitecture.Application.Common.Behaviours;

namespace CleanArchitecture.WebUI.Controllers;

[Authorize]
public class TodoItemsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedList<TodoItemBriefDto>>> GetTodoItemsWithPagination(
        [FromQuery] GetTodoItemsWithPaginationQuery query,
        RequestHandler<GetTodoItemsWithPaginationQuery, PaginatedList<TodoItemBriefDto>> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(query, cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(
        CreateTodoItemCommand command,
        RequestHandler<CreateTodoItemCommand, int> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(command, cancellationToken);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Update(
        int id,
        UpdateTodoItemCommand command,
        RequestHandler<UpdateTodoItemCommand> handler,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await handler.Handle(command, cancellationToken);

        return NoContent();
    }

    [HttpPut("[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> UpdateItemDetails(
        int id,
        UpdateTodoItemDetailCommand command,
        RequestHandler<UpdateTodoItemDetailCommand> handler,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest();
        }

        await handler.Handle(command, cancellationToken);

        return NoContent();
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Delete(int id, RequestHandler<DeleteTodoItemCommand> handler, CancellationToken cancellationToken)
    {
        await handler.Handle(new DeleteTodoItemCommand(id), cancellationToken);

        return NoContent();
    }
}
