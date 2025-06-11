using CleanArchitecture.Application.TodoLists.Queries.GetTodos;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.ValueObjects;

namespace CleanArchitecture.Application.FunctionalTests.TodoLists.Queries;

using static Testing;

public class GetTodosTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnPriorityLevels()
    {
        await RunAsDefaultUserAsync();

        var query = new GetTodosQuery();

        var result = await SendAsync<GetTodosQuery, TodosVm>(query);

        Assert.That(result.PriorityLevels, Is.Not.Empty);
    }

    [Test]
    public async Task ShouldReturnAllListsAndItems()
    {
        await RunAsDefaultUserAsync();

        await AddAsync(new TodoList
        {
            Title = "Shopping",
            Colour = Colour.Blue,
            Items =
                    {
                        new TodoItem { Title = "Apples", Done = true },
                        new TodoItem { Title = "Milk", Done = true },
                        new TodoItem { Title = "Bread", Done = true },
                        new TodoItem { Title = "Toilet paper" },
                        new TodoItem { Title = "Pasta" },
                        new TodoItem { Title = "Tissues" },
                        new TodoItem { Title = "Tuna" }
                    }
        });

        var query = new GetTodosQuery();

        var result = await SendAsync<GetTodosQuery, TodosVm>(query);

        Assert.That(result.Lists, Has.Count.EqualTo(1));
        Assert.That(result.Lists.First().Items, Has.Count.EqualTo(7));
    }

    [Test]
    public void ShouldDenyAnonymousUser()
    {
        var query = new GetTodosQuery();

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await SendAsync<GetTodosQuery, TodosVm>(query));
    }
}
