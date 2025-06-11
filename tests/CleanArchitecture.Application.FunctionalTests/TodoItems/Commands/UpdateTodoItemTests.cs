using CleanArchitecture.Application.TodoItems.Commands.CreateTodoItem;
using CleanArchitecture.Application.TodoItems.Commands.UpdateTodoItem;
using CleanArchitecture.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.FunctionalTests.TodoItems.Commands;

using static Testing;

public class UpdateTodoItemTests : BaseTestFixture
{
    [Test]
    public void ShouldRequireValidTodoItemId()
    {
        var command = new UpdateTodoItemCommand { Id = 99, Title = "New Title" };
        Assert.ThrowsAsync<NotFoundException>(async () => await SendAsync(command));
    }

    [Test]
    public async Task ShouldUpdateTodoItem()
    {
        var userId = await RunAsDefaultUserAsync();

        var listId = await SendAsync<CreateTodoListCommand, int>(new CreateTodoListCommand
        {
            Title = "New List"
        });

        var itemId = await SendAsync<CreateTodoItemCommand, int>(new CreateTodoItemCommand
        {
            ListId = listId,
            Title = "New Item"
        });

        var command = new UpdateTodoItemCommand
        {
            Id = itemId,
            Title = "Updated Item Title"
        };

        await SendAsync(command);

        var item = await FindAsync<TodoItem>(itemId);

        Assert.That(item, Is.Not.Null);
        Assert.That(item!.Title, Is.EqualTo(command.Title));
        Assert.That(item.LastModifiedBy, Is.Not.Null);
        Assert.That(item.LastModifiedBy, Is.EqualTo(userId));
        Assert.That(item.LastModified, Is.EqualTo(DateTimeOffset.Now).Within(TimeSpan.FromMilliseconds(10000)));
    }
}
