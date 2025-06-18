using FavoriteQuoutesWebApi.Models;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class BooksController : ControllerBase
{
    [HttpGet]
    public IEnumerable<Book> GetBooks() =>
    [
        new Book { Id = 1, Title = "Book 1" },
        new Book { Id = 2, Title = "Book 2" },
        new Book { Id = 3, Title = "Book 3" }
    ];

    [HttpGet("{id}")]
    public Book GetBook(int id) {
        return new Book { Id = 1, Title = "Book 1" };
    }

    [HttpPost]
    public IActionResult CreateBook([FromBody] Book book)
    {
        Console.WriteLine($"Created book: {book.Title}");

        return Ok(book);
    }

    [HttpPut("{id}")]
    public ActionResult<Book> EditBook(int id)
    {
        Console.WriteLine($"Edited book: {id}");

        return Ok(id);
    }

    [HttpDelete("{id}")]
    public ActionResult<Book> DeleteBook(int id)
    {
        Console.WriteLine($"Edited book: {id}");

        return Ok(id);
    }
}