using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Application.TodoLists.Commands.PurgeTodoLists;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.FunctionalTests.TodoLists.Commands;

using static Testing;

public class PurgeTodoListsTests : BaseTestFixture
{
    [Test]
    public void ShouldDenyAnonymousUser()
    {
        var command = new PurgeTodoListsCommand();

        var hasAuthorize = command.GetType()
            .GetCustomAttributes(typeof(AuthorizeAttribute), true)
            .Any();
        Assert.That(hasAuthorize, Is.True);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await SendAsync(command));
    }

    [Test]
    public async Task ShouldDenyNonAdministrator()
    {
        await RunAsDefaultUserAsync();

        var command = new PurgeTodoListsCommand();

        Assert.ThrowsAsync<ForbiddenAccessException>(async () => await SendAsync(command));
    }

    [Test]
    public async Task ShouldAllowAdministrator()
    {
        await RunAsAdministratorAsync();

        var command = new PurgeTodoListsCommand();

        Assert.DoesNotThrowAsync(async () => await SendAsync(command));
    }

    [Test]
    public async Task ShouldDeleteAllLists()
    {
        await RunAsAdministratorAsync();

        await SendAsync<CreateTodoListCommand, int>(new CreateTodoListCommand
        {
            Title = "New List #1"
        });

        await SendAsync<CreateTodoListCommand, int>(new CreateTodoListCommand
        {
            Title = "New List #2"
        });

        await SendAsync<CreateTodoListCommand, int>(new CreateTodoListCommand
        {
            Title = "New List #3"
        });

        await SendAsync(new PurgeTodoListsCommand());

        var count = await CountAsync<TodoList>();

        Assert.That(count, Is.EqualTo(0));
    }
}
