using CleanArchitecture.Application.TodoItems.Commands.CreateTodoItem;
using CleanArchitecture.Application.TodoItems.Commands.UpdateTodoItem;
using CleanArchitecture.Application.TodoItems.Commands.UpdateTodoItemDetail;
using CleanArchitecture.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;

namespace CleanArchitecture.Application.FunctionalTests.TodoItems.Commands;

using static Testing;

public class UpdateTodoItemDetailTests : BaseTestFixture
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

        var command = new UpdateTodoItemDetailCommand
        {
            Id = itemId,
            ListId = listId,
            Note = "This is the note.",
            Priority = PriorityLevel.High
        };

        await SendAsync(command);

        var item = await FindAsync<TodoItem>(itemId);

        Assert.That(item, Is.Not.Null);
        Assert.That(item!.ListId, Is.EqualTo(command.ListId));
        Assert.That(item.Note, Is.EqualTo(command.Note));
        Assert.That(item.Priority, Is.EqualTo(command.Priority));
        Assert.That(item.LastModifiedBy, Is.Not.Null);
        Assert.That(item.LastModifiedBy, Is.EqualTo(userId));
        Assert.That(item.LastModified, Is.EqualTo(DateTimeOffset.Now).Within(TimeSpan.FromMilliseconds(10000)));
    }
}
