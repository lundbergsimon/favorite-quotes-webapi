using FavoriteQuoutesWebApi.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class BooksController : ControllerBase
{
    // In-memory list to simulate a data store TODO: Replace with a database
    private static List<Book> books = new List<Book>();
    private static int nextId = 1;

    [HttpGet]
    public IEnumerable<Book> GetBooks()
    {
        // TODO: Replace with a database
        return books;
    }

    [HttpGet("{id}")]
    public ActionResult<Book> GetBook(int id)
    {
        // TODO: Replace with a database
        var book = books.FirstOrDefault(b => b.Id == id);
        if (book == null)
            return NotFound();

        return Ok(book);
    }

    [HttpPost]
    public IActionResult CreateBook([FromBody] Book book)
    {
        Console.WriteLine("Creating book: " + System.Text.Json.JsonSerializer.Serialize(book));
        // TODO: Replace with a database
        book.Id = nextId++;
        books.Add(book);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    [HttpPut("{id}")]
    public IActionResult EditBook(int id, Book updatedBook)
    {
        Console.WriteLine("Updating book: " + System.Text.Json.JsonSerializer.Serialize(updatedBook));

        // TODO: Replace with a database
        var book = books.FirstOrDefault(b => b.Id == id);
        if (book == null)
            return NotFound();

        book.Title = updatedBook.Title;
        book.Author = updatedBook.Author;

        return NoContent();
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteBook(int id)
    {
        var book = books.FirstOrDefault(b => b.Id == id);
        if (book == null)
            return NotFound();

        books.Remove(book);
        return NoContent();
    }
}