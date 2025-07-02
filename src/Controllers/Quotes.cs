using FavoriteQuoutesWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FavoriteQuoutesWebApi.Storage;

namespace FavoriteQuoutesWebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class QuotesController : ControllerBase
{
    private readonly IQuoteStore _quoteStore;
    private readonly IUserStore _userStore;

    public QuotesController(IQuoteStore quoteStore, IUserStore userStore)
    {
        _quoteStore = quoteStore;
        _userStore = userStore;
    }

    /// <summary>
    /// Retrieves all quotes for the current user from the store.
    /// </summary>
    /// <returns>A list of all quotes currently stored for the user.</returns>
    [HttpGet]
    public IEnumerable<Quote> GetQuotes()
    {
        var userId = GetUserId();
        if (userId == null) return [];
        if (_userStore.GetById(userId.Value) == null) return [];
        return _quoteStore.GetByUserId(userId.Value);
    }

    /// <summary>
    /// Retrieves a quote by its id from the in-memory collection.
    /// </summary>
    /// <param name="id">The id of the quote to retrieve.</param>
    /// <returns>The quote if found, otherwise a 404 status code.</returns>
    [HttpGet("{id}")]
    public ActionResult<Quote> GetQuote(int id)
    {
        var userId = GetUserId();
        if (userId == null || _userStore.GetById(userId.Value) == null)
            return NotFound();
        var quote = _quoteStore.GetById(userId.Value, id);
        if (quote == null)
            return NotFound();
        return Ok(quote);
    }

    /// <summary>
    /// Creates a new quote entry in the in-memory collection.
    /// </summary>
    /// <param name="quote">The quote object to be created.</param>
    /// <returns>A 201 Created response with the newly created quote.</returns>

    [HttpPost]
    public IActionResult CreateQuote([FromBody] QuoteCreateRequest quote)
    {
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();
        var newQuote = new Quote
        {
            // Id should be set by the store
            Text = quote.Text,
            BookId = quote.BookId,
        };
        _quoteStore.Add(userId.Value, newQuote);
        return CreatedAtAction(nameof(GetQuote), new { id = newQuote.Id }, newQuote);
    }

    /// <summary>
    /// Deletes a quote from the in-memory collection with the given id.
    /// </summary>
    /// <param name="id">The id of the quote to delete.</param>
    /// <returns>A 204 No Content response if the quote was found and deleted, otherwise a 404 status code.</returns>
    [HttpDelete("{id}")]
    public IActionResult DeleteQuote(int id)
    {
        var userId = GetUserId();
        if (userId == null || _userStore.GetById(userId.Value) == null)
            return NotFound();
        var quote = _quoteStore.GetById(userId.Value, id);
        if (quote == null)
            return NotFound();
        _quoteStore.Remove(userId.Value, id);
        return NoContent();
    }

    // Helper to get user id from JWT
    private Guid? GetUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
            return userId;
        return null;
    }
}
