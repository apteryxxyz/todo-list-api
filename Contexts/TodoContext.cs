using Microsoft.EntityFrameworkCore;

namespace Backend.Contexts;

public class TodoContext : DbContext
{
    public DbSet<TodoList> Lists { get; set; } = default!;
    public DbSet<TodoItem> Items { get; set; } = default!;

    public TodoContext(DbContextOptions<TodoContext> options)
        : base(options)
    {
    }
}

public class TodoList
{
    public string Id { get; set; } = default!;

    public string Key { get; set; } = default!;

    public string Name { get; set; } = default!;
}

public class TodoListIncoming
{
    public string Name { get; set; } = default!;
}

public class TodoListOutgoing
{
    public string Id { get; set; } = default!;

    public string? Key { get; set; }

    public string Name { get; set; } = default!;

    public TodoListOutgoing(TodoList? list, bool includeKey)
    {
        if (list == null) return;

        Id = list.Id;
        Name = list.Name;
        if (includeKey) Key = list.Key;
    }
}

public class TodoItem
{
    public string Id { get; set; } = default!;
    
    public string ListId { get; set; } = default!;

    public string Content { get; set; } = default!;

    public bool IsComplete { get; set; } = default!;
}

public class TodoItemIncoming
{
    public string Content { get; set; } = default!;

    public bool IsComplete { get; set; } = default!;
}

public class TodoItemOutgoing
{
    public string Id { get; set; } = default!;
    
    public string ListId { get; set; } = default!;

    public string Content { get; set; } = default!;

    public bool IsComplete { get; set; } = default!;

    public TodoItemOutgoing(TodoItem? item)
    {
        if (item == null) return;
        
        ListId = item.ListId;
        Content = item.Content;
        IsComplete = item.IsComplete;
    }
}
