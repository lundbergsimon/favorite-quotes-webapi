using FavoriteQuoutesWebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FavoriteQuoutesWebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class QuotesController : ControllerBase
{
    // Store quotes per user
    private static Dictionary<Guid, List<Quote>> userQuotes = new Dictionary<Guid, List<Quote>>();
    private static int nextId = 1;

    /// <summary>
    /// Retrieves all quotes from the in-memory collection.
    /// </summary>
    /// <returns>A list of all quotes currently stored.</returns>
    [HttpGet]
    public IEnumerable<Quote> GetQuotes()
    {
        var userId = GetUserId();
        if (userId == null) return Enumerable.Empty<Quote>();
        if (!userQuotes.ContainsKey(userId.Value)) return Enumerable.Empty<Quote>();
        return userQuotes[userId.Value];
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
        if (userId == null || !userQuotes.ContainsKey(userId.Value))
            return NotFound();
        var quote = userQuotes[userId.Value].FirstOrDefault(q => q.Id == id);
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
            Id = nextId++,
            Text = quote.Text,
            BookId = quote.BookId,
        };
        if (!userQuotes.ContainsKey(userId.Value))
            userQuotes[userId.Value] = new List<Quote>();
        userQuotes[userId.Value].Add(newQuote);
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
        if (userId == null || !userQuotes.ContainsKey(userId.Value))
            return NotFound();
        var quote = userQuotes[userId.Value].FirstOrDefault(q => q.Id == id);
        if (quote == null)
            return NotFound();
        userQuotes[userId.Value].Remove(quote);
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
