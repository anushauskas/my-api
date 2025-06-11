using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.TodoLists.Commands.CreateTodoList;
using CleanArchitecture.Domain.Entities;

namespace CleanArchitecture.Application.FunctionalTests.TodoLists.Commands;

using static Testing;

public class CreateTodoListTests : BaseTestFixture
{
    [Test]
    public void ShouldRequireMinimumFields()
    {
        var command = new CreateTodoListCommand();
        Assert.ThrowsAsync<ValidationException>(async () => await SendAsync<CreateTodoListCommand, int>(command));
    }

    [Test]
    public async Task ShouldRequireUniqueTitle()
    {
        await SendAsync<CreateTodoListCommand, int>(new CreateTodoListCommand
        {
            Title = "Shopping"
        });

        var command = new CreateTodoListCommand
        {
            Title = "Shopping"
        };

        var ex = Assert.ThrowsAsync<ValidationException>(async () => await SendAsync<CreateTodoListCommand, int>(command));
        Assert.That(ex.Errors, Is.Not.Null);
        Assert.That(ex.Errors, Does.ContainKey("Title"));
        Assert.That(ex.Errors["Title"], Does.Contain("'Title' must be unique."));
    }

    [Test]
    public async Task ShouldCreateTodoList()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new CreateTodoListCommand
        {
            Title = "Tasks"
        };

        var id = await SendAsync<CreateTodoListCommand, int>(command);

        var list = await FindAsync<TodoList>(id);

        Assert.That(list, Is.Not.Null);
        Assert.That(list!.Title, Is.EqualTo(command.Title));
        Assert.That(list.CreatedBy, Is.EqualTo(userId));
        Assert.That(list.Created, Is.EqualTo(DateTimeOffset.Now).Within(TimeSpan.FromMilliseconds(10000)));
    }
}
