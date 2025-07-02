using FavoriteQuoutesWebApi.Models;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private static List<Book> books = new List<Book>();
    private static int nextId = 1;

    /// <summary>
    /// Retrieves all books from the database.
    /// </summary>
    /// <returns>A list of all books in the database.</returns>
    [HttpGet]
    public IEnumerable<Book> GetBooks()
    {
        return books;
    }

    /// <summary>
    /// Retrieves a book by its id from the database.
    /// </summary>
    /// <param name="id">The id of the book to retrieve.</param>
    /// <returns>The book if found, otherwise a 404 status code.</returns>
    [HttpGet("{id}")]
    public ActionResult<Book> GetBook(int id)
    {
        var book = books.FirstOrDefault(b => b.Id == id);
        if (book == null)
            return NotFound();

        return Ok(book);
    }

    /// <summary>
    /// Creates a new book entry in the database.
    /// </summary>
    /// <param name="book">The book object to be created.</param>
    /// <returns>A 201 Created response with the newly created book.</returns>
    [HttpPost]
    public IActionResult CreateBook([FromBody] Book book)
    {
        book.Id = nextId++;
        books.Add(book);
        return CreatedAtAction(nameof(GetBook), new { id = book.Id }, book);
    }

    /// <summary>
    /// Edits a book in the database with the given id and updated title and author.
    /// </summary>
    /// <param name="id">The id of the book to edit.</param>
    /// <param name="updatedBook">The book object containing the updated title and author.</param>
    /// <returns>A 204 No Content response if the book was found and edited, otherwise a 404 status code.</returns>
    [HttpPut("{id}")]
    public IActionResult EditBook(int id, Book updatedBook)
    {
        var book = books.FirstOrDefault(b => b.Id == id);
        if (book == null)
            return NotFound();

        book.Title = updatedBook.Title;
        book.Author = updatedBook.Author;

        return NoContent();
    }

    /// <summary>
    /// Deletes a book from the database with the given id.
    /// </summary>
    /// <param name="id">The id of the book to delete.</param>
    /// <returns>A 204 No Content response if the book was found and deleted, otherwise a 404 status code.</returns>
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