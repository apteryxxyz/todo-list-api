using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Backend.Contexts;

namespace Backend.Controllers;

/// <summary>
/// This is the controller for the <see cref="TodoList"/> model.
/// </summary>
[Route("api/lists")]
[ApiController]
public class ListController : Controller
{
    private readonly TodoContext _context;
    private readonly ILogger<ListController> _logger;

    public ListController(TodoContext context, ILogger<ListController> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Create a new todo list.
    /// </summary>
    /// <param name="name">The name for the list.</param>
    /// <returns>A todo list object WITH its API key.</returns>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<TodoListOutgoing>> CreateList(string name)
    {
        // Generate random strings to use as ID and API key
        var id = Guid.NewGuid().ToString("N");
        var key = Guid.NewGuid().ToString("N");

        // Create and save new todo list object
        var list = new TodoList { Id = id, Key = key, Name = name };
        _context.Lists.Add(list);
        await _context.SaveChangesAsync();

        // Return object
        return CreatedAtAction
            (
                nameof(GetList),
                new { listId = id, key = key },
                new TodoListOutgoing(list, true)
            );
    }

    /// <summary>
    /// Get a todo list by its ID.
    /// </summary>
    /// <param name="listId">List ID.</param>
    /// <param name="key">API Key.</param>
    /// <returns>A todo list object WITHOUT its API key.</returns>
    [HttpGet]
    [Route("{listId}")]
    public async Task<ActionResult<TodoListOutgoing>> GetList(string listId, string key)
    {
        // FInd todo list by ID
        var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == listId);
        if (list is null) return NotFound();

        // Ensure API key is correct
        if (list.Key != key) return Unauthorized();

        return new TodoListOutgoing(list, false);
    }

    /// <summary>
    /// Update the properties of a todo list.
    /// </summary>
    /// <param name="listId">List ID.</param>
    /// <param name="key">API Key.</param>
    /// <param name="data">New data object.</param>
    /// <returns>The updated todo list object.</returns>
    [HttpPatch]
    [Route("{listId}")]
    public async Task<ActionResult<TodoListOutgoing>> UpdateList(string listId, string key, [FromBody] TodoListIncoming data)
    {
        // Find todo list by ID
        var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == listId);
        if (list is null) return NotFound();

        // Ensure API key is correct
        if (list.Key != key) return Unauthorized();

        // Update data
        list.Name = data.Name;
        await _context.SaveChangesAsync();

        // Return object
        return new TodoListOutgoing(list, true);
    }

    /// <summary>
    /// Delete an existing todo list.
    /// </summary>
    /// <param name="listId">List ID.</param>
    /// <param name="key">API Key.</param>
    /// <returns>OK.</returns>
    [HttpDelete]
    [Route("{listId}")]
    public async Task<IActionResult> DeleteList(string listId, string key)
    {
        // FInd todo list by ID
        var list = await _context.Lists.FirstOrDefaultAsync(l => l.Id == listId);
        if (list is null) return NotFound();

        // Ensure API key is correct
        if (list.Key != key) return Unauthorized();

        // Remove list
        _context.Lists.Remove(list);

        // Remove list items
        var items = await _context.Items.Where(i => i.ListId == listId).ToListAsync();
        _context.Items.RemoveRange(items);

        // Save changes
        await _context.SaveChangesAsync();

        return Ok();
    }
}
