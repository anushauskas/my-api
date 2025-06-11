using CleanArchitecture.Application.Common.Interfaces;
using CleanArchitecture.Application.Common.Models;
using CleanArchitecture.Application.Common.Security;
using CleanArchitecture.Domain.Entities;
using CleanArchitecture.Domain.Enums;

namespace CleanArchitecture.Application.TodoLists.Queries.GetTodos;

[Authorize]
public record GetTodosQuery;

public class GetTodosQueryHandler : IRequestHandler<GetTodosQuery, TodosVm>
{
    private readonly IApplicationDbContext _context;

    public GetTodosQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<TodosVm> Handle(GetTodosQuery request, CancellationToken cancellationToken)
    {
        Func<TodoList, TodoListDto> listsSelector = x => new TodoListDto
        {
            Id = x.Id,
            Title = x.Title,
            Colour = x.Colour.ToString(),
            Items = x.Items.Select(i => new TodoItemDto
            {
                Id = i.Id,
                ListId = i.ListId,
                Title = i.Title,
                Done = i.Done,
                Priority = (int)i.Priority,
                Note = i.Note
            }).ToList().AsReadOnly()
        };

        var todoLists = await _context.TodoLists
                .AsNoTracking()
                .Include(t => t.Items)
                .OrderBy(t => t.Title)
                .ToListAsync(cancellationToken);

        return new TodosVm
        {
            PriorityLevels = Enum.GetValues(typeof(PriorityLevel))
                .Cast<PriorityLevel>()
                .Select(p => new LookupDto { Id = (int)p, Title = p.ToString() })
                .ToList(),

            Lists = todoLists.Select(listsSelector).ToList().AsReadOnly()
        };
    }
}
