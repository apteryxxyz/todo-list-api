using Microsoft.AspNetCore.Mvc;
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

    /// <summary>
    /// Get the items in a list.
    /// </summary>
    /// <param name="listId">List ID.</param>
    /// <param name="key">API key.</param>
    /// <returns>Array of items.</returns>
    [HttpGet]
    [Route("{listId}/items")]
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

    /// <summary>
    /// Add a new item to the a list.
    /// </summary>
    /// <param name="listId">List ID.</param>
    /// <param name="key">API key.</param>
    /// <param name="data">Item data.</param>
    /// <returns>New item object.</returns>
    [HttpPut]
    [Route("{listId}/items")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<TodoItemOutgoing>> CreateItem(string listId, string key, [FromBody] TodoItemIncoming data)
    {
        // Find todo list by ID
        var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == listId);
        if (list is null) return NotFound();

        // Ensure API key is correct
        if (list.Key != key) return Unauthorized();

        // Generate random string to use as ID
        var itemId = Guid.NewGuid().ToString("N");

        // Create new item
        var item = new TodoItem
        {
            Id = itemId,
            ListId = listId,
            Content = data.Content,
            IsComplete = data.IsComplete
        };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        return CreatedAtAction
            (
                nameof(GetItem),
                new { listId = listId, itemId = itemId, key = key },
                new TodoItemOutgoing(item)
            );
    }

    /// <summary>
    /// Get an item from a list.
    /// </summary>
    /// <param name="listId">List ID.</param>
    /// <param name="itemId">Item ID.</param>
    /// <param name="key">API Key.</param>
    /// <returns>Item object.</returns>
    [HttpGet]
    [Route("{listId}/items/{itemId}")]
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

    /// <summary>
    /// Update an item in a list.
    /// </summary>
    /// <param name="listId">List ID.</param>
    /// <param name="itemId">Item ID.</param>
    /// <param name="key">API Key.</param>
    /// <param name="data">New item data.</param>
    /// <returns>Item object.</returns>
    [HttpPatch]
    [Route("{listId}/items/{itemId}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
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

    /// <summary>
    /// Delete an item from a list.
    /// </summary>
    /// <param name="listId">List ID.</param>
    /// <param name="itemId">Item ID.</param>
    /// <param name="key">API Key.</param>
    /// <returns>OK.</returns>
    [HttpDelete]
    [Route("{listId}/items/{itemId}")]
    public async Task<IActionResult> DeleteItem(string listId, string itemId, string key)
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

        return Ok();
    }
}
