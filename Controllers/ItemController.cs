﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Contexts;

namespace Backend.Controllers;

/// <summary>
/// This is the controller for the <see cref="TodoItem"/> model.
/// </summary>
[Route("api/lists")]
[ApiController]
public class ItemController : Controller
{
    private readonly TodoContext _context;
    private readonly ILogger<ItemController> _logger;

    public ItemController(TodoContext context, ILogger<ItemController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet]
    [Route("{ListId}/items")]
    public async Task<ActionResult<IEnumerable<TodoItemOutgoing>>> GetItems(string listId, string key)
    {
        // FInd todo list by ID
        var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == listId);
        if (list is null) return NotFound();

        // Ensure API key is correct
        if (list.Key != key) return Unauthorized();

        // Return list's items
        var items = await _context.Items.Where(i => i.ListId == listId).ToListAsync();
        return items.Select(i => new TodoItemOutgoing(i)).ToList();
    }

    [HttpPut]
    [Route("{ListId}/items")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<TodoItemOutgoing>> CreateItem(string listId, string key, string content)
    {
        // Find todo list by ID
        var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == listId);
        if (list is null) return NotFound();

        // Ensure API key is correct
        if (list.Key != key) return Unauthorized();

        // Generate random string to use as ID
        var id = Guid.NewGuid().ToString("N");

        // Create new item
        var item = new TodoItem
        {
            Id = id,
            ListId = listId,
            Content = content,
            IsComplete = false
        };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        return new TodoItemOutgoing(item);
    }

    [HttpGet]
    [Route("{ListId}/items/{ItemId}")]
    public async Task<ActionResult<TodoItemOutgoing>> GetItem(string listId, string itemId, string key)
    {
        // Find todo list by ID
        var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == listId);
        if (list is null) return NotFound();

        // Ensure API key is correct
        if (list.Key != key) return Unauthorized();

        // Find item by ID
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
        if (item is null) return NotFound();

        // Return item
        return new TodoItemOutgoing(item);
    }

    [HttpPatch]
    [Route("{ListId}/items/{ItemId}")]
    public async Task<ActionResult<TodoItemOutgoing>> UpdateItem(string listId, string itemId, string key, [FromBody] TodoItemIncoming data)
    {
        // Find todo list by ID
        var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == listId);
        if (list is null) return NotFound();

        // Ensure API key is correct
        if (list.Key != key) return Unauthorized();

        // Find item by ID
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
        if (item is null) return NotFound();

        // Update data
        item.Content = data.Content;
        item.IsComplete = data.IsComplete;
        await _context.SaveChangesAsync();

        return new TodoItemOutgoing(item);
    }

    [HttpDelete]
    [Route("{ListId}/items/{ItemId}")]
    public async Task<ActionResult<TodoItemOutgoing>> DeleteItem(string listId, string itemId, string key)
    {
        // Find todo list by ID
        var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == listId);
        if (list is null) return NotFound();

        // Ensure API key is correct
        if (list.Key != key) return Unauthorized();

        // Find item by ID
        var item = await _context.Items.FirstOrDefaultAsync(i => i.Id == itemId);
        if (item is null) return NotFound();

        // Remove item
        _context.Items.Remove(item);
        await _context.SaveChangesAsync();

        return new TodoItemOutgoing(item);
    }
}