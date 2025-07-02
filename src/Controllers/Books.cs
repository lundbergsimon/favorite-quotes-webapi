using FavoriteQuoutesWebApi.Models;
using Microsoft.AspNetCore.Mvc;

using Microsoft.AspNetCore.Authorization;
using FavoriteQuoutesWebApi.Storage;

[ApiController]
[Route("[controller]")]
[Authorize]
public class BooksController : ControllerBase
{
    private readonly IBookStore _bookStore;
    private readonly IUserStore _userStore;
    // Store books per user
    // private static Dictionary<Guid, List<Book>> userBooks = new Dictionary<Guid, List<Book>>();
    // private static int nextId = 1;

    public BooksController(IBookStore bookStore, IUserStore userStore)
    {
        _bookStore = bookStore;
        _userStore = userStore;
    }

    /// <summary>
    /// Retrieves all books from the store for the current user.
    /// </summary>
    /// <returns>A list of all books for the user.</returns>
    [HttpGet]
    public IEnumerable<Book> GetBooks()
    {
        var userId = GetUserId();
        if (userId == null) return [];
        if (_userStore.GetById(userId.Value) == null) return [];
        return _bookStore.GetByUserId(userId.Value);
    }

    /// <summary>
    /// Retrieves a book by its id from the database.
    /// </summary>
    /// <param name="id">The id of the book to retrieve.</param>
    /// <returns>The book if found, otherwise a 404 status code.</returns>
    [HttpGet("{id}")]
    public ActionResult<Book> GetBook(int id)
    {
        var userId = GetUserId();
        if (userId == null || _userStore.GetById(userId.Value) == null)
            return NotFound();
        var book = _bookStore.GetById(userId.Value, id);
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
        var userId = GetUserId();
        if (userId == null)
            return Unauthorized();

        _bookStore.Add(userId.Value, book);

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
        var userId = GetUserId();
        if (userId == null || _userStore.GetById(userId.Value) == null)
            return NotFound();
        var book = _bookStore.GetById(userId.Value, id);
        if (book == null)
            return NotFound();
        book.Title = updatedBook.Title;
        book.Author = updatedBook.Author;
        book.PublishDate = updatedBook.PublishDate;
        _bookStore.Update(userId.Value, book);
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
        var userId = GetUserId();
        if (userId == null || _userStore.GetById(userId.Value) == null)
            return NotFound();
        var book = _bookStore.GetById(userId.Value, id);
        if (book == null)
            return NotFound();
        _bookStore.Remove(userId.Value, id);
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