using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Application.TodoLists.Commands.UpdateTodoList;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.FunctionalTests.TodoLists.Commands;

using static Testing;

public class UpdateTodoListTests : BaseTestFixture
{
    [Test]
    public void ShouldRequireValidTodoListId()
    {
        var command = new UpdateTodoListCommand { Id = 99, Title = "New Title" };
        Assert.ThrowsAsync<NotFoundException>(async () => await SendAsync(command));
    }

    [Test]
    public async Task ShouldRequireUniqueTitle()
    {
        var listId = await SendAsync<CreateTodoListCommand, int>(new CreateTodoListCommand
        {
            Title = "New List"
        });

        await SendAsync<CreateTodoListCommand, int>(new CreateTodoListCommand
        {
            Title = "Other List"
        });

        var command = new UpdateTodoListCommand
        {
            Id = listId,
            Title = "Other List"
        };

        var ex = Assert.ThrowsAsync<ValidationException>(async () => await SendAsync(command));
        Assert.That(ex.Errors, Is.Not.Null);
        Assert.That(ex.Errors, Does.ContainKey("Title"));
        Assert.That(ex.Errors["Title"], Does.Contain("'Title' must be unique."));
    }

    [Test]
    public async Task ShouldUpdateTodoList()
    {
        var userId = await RunAsDefaultUserAsync();

        var listId = await SendAsync<CreateTodoListCommand, int>(new CreateTodoListCommand
        {
            Title = "New List"
        });

        var command = new UpdateTodoListCommand
        {
            Id = listId,
            Title = "Updated List Title"
        };

        await SendAsync(command);

        var list = await FindAsync<TodoList>(listId);

        Assert.That(list, Is.Not.Null);
        Assert.That(list!.Title, Is.EqualTo(command.Title));
        Assert.That(list.LastModifiedBy, Is.Not.Null);
        Assert.That(list.LastModifiedBy, Is.EqualTo(userId));
        Assert.That(list.LastModified, Is.EqualTo(DateTimeOffset.Now).Within(TimeSpan.FromMilliseconds(10000)));
    }
}
