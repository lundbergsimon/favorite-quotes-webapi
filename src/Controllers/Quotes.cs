using FavoriteQuoutesWebApi.Models;
using Microsoft.AspNetCore.Mvc;

namespace FavoriteQuoutesWebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class QuotesController : ControllerBase
{
    private static List<Quote> quotes = new List<Quote>();
    private static int nextId = 1;

    [HttpGet]
    public IEnumerable<Quote> GetQuotes()
    {
        // TODO: Replace with a database
        return quotes;
    }

    [HttpGet("{id}")]
    public ActionResult<Quote> GetQuote(int id)
    {
        // TODO: Replace with a database
        var quote = quotes.FirstOrDefault(q => q.Id == id);
        if (quote == null)
            return NotFound();
        return quote;
    }

    [HttpPost]
    public IActionResult CreateQuote([FromBody] QuoteCreateRequest quote)
    {
        Console.WriteLine("Creating quote: " + System.Text.Json.JsonSerializer.Serialize(quote));
        var newQuote = new Quote
        {
            Id = nextId++,
            Text = quote.Text,
            BookId = quote.BookId,
        };
        // TODO: Replace with a database
        quotes.Add(newQuote);
        return CreatedAtAction(nameof(GetQuote), new { id = newQuote.Id }, newQuote);
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteQuote(int id)
    {
        var quote = quotes.FirstOrDefault(q => q.Id == id);
        if (quote == null)
            return NotFound();
        quotes.Remove(quote);
        return NoContent();
    }
}
