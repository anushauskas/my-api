using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.TodoItems.Commands.CreateTodoItem;
using CleanArchitecture.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.FunctionalTests.TodoItems.Commands;

using static Testing;

public class CreateTodoItemTests : BaseTestFixture
{
    [Test]
    public void ShouldRequireMinimumFields()
    {
        var command = new CreateTodoItemCommand();

        Assert.ThrowsAsync<ValidationException>(async () =>
            await SendAsync<CreateTodoItemCommand, int>(command));
    }

    [Test]
    public async Task ShouldCreateTodoItem()
    {
        var userId = await RunAsDefaultUserAsync();

        var listId = await SendAsync<CreateTodoListCommand, int>(new CreateTodoListCommand
        {
            Title = "New List"
        });

        var command = new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "Tasks"
        };

        var itemId = await SendAsync<CreateTodoItemCommand, int>(command);

        var item = await FindAsync<TodoItem>(itemId);
        Assert.That(item, Is.Not.Null);
        Assert.That(item!.ListId, Is.EqualTo(command.ListId));
        Assert.That(item.Title, Is.EqualTo(command.Title));
        Assert.That(item.CreatedBy, Is.EqualTo(userId));
        Assert.That(item.Created, Is.EqualTo(DateTimeOffset.Now).Within(TimeSpan.FromMilliseconds(10000)));
        Assert.That(item.LastModifiedBy, Is.EqualTo(userId));
        Assert.That(item.LastModified, Is.EqualTo(DateTimeOffset.Now).Within(TimeSpan.FromMilliseconds(10000)));
    }
}
