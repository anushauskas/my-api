using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Application.TodoLists.Commands.DeleteTodoList;
using CleanArchitecture.Application.TodoLists.Commands.PurgeTodoLists;
using CleanArchitecture.Application.TodoLists.Commands.UpdateTodoList;
using CleanArchitecture.Application.TodoLists.Queries.GetTodos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanArchitecture.WebUI.Controllers;

[Authorize]
public class TodoListsController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<TodosVm>> Get(
        RequestHandler<GetTodosQuery, TodosVm> handler,
        CancellationToken cancellationToken)
    {
        return await handler.Handle(new GetTodosQuery(), cancellationToken);
    }

    [HttpPost]
    public async Task<ActionResult<int>> Create(
        CreateTodoListCommand command,
        RequestHandler<CreateTodoListCommand, int> handler,
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
        UpdateTodoListCommand command,
        RequestHandler<UpdateTodoListCommand> handler,
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
    public async Task<IActionResult> Delete(
        int id,
        RequestHandler<DeleteTodoListCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new DeleteTodoListCommand(id), cancellationToken);

        return NoContent();
    }

    [HttpDelete("Purge")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Purge(
        RequestHandler<PurgeTodoListsCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new PurgeTodoListsCommand(), cancellationToken);

        return NoContent();
    }
}
