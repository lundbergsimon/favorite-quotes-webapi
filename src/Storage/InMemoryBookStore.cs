namespace FavoriteQuoutesWebApi.Storage
{
    using FavoriteQuoutesWebApi.Models;

    public class InMemoryBookStore : IBookStore
    {
        private readonly Dictionary<Guid, List<Book>> _userBooks = [];

        public IEnumerable<Book> GetByUserId(Guid userId) => _userBooks.TryGetValue(userId, out List<Book>? value) ? value : Enumerable.Empty<Book>();
        public Book? GetById(Guid userId, int bookId) => _userBooks.TryGetValue(userId, out List<Book>? value) ? value.FirstOrDefault(b => b.Id == bookId) : null;

        /// <summary>
        /// Adds a book to the in-memory collection for the given user.
        /// </summary>
        /// <param name="userId">The id of the user to add the book for.</param>
        /// <param name="book">The book to add to the collection.</param>
        public void Add(Guid userId, Book book)
        {
            if (!_userBooks.ContainsKey(userId)) _userBooks[userId] = [];
            _userBooks[userId].Add(book);
        }

        /// <summary>
        /// Removes a book from the in-memory collection with the given id and user id.
        /// </summary>
        /// <param name="userId">The id of the user to remove the book for.</param>
        /// <param name="bookId">The id of the book to remove.</param>
        public void Remove(Guid userId, int bookId)
        {
            if (_userBooks.TryGetValue(userId, out List<Book>? value))
            {
                var book = value.FirstOrDefault(b => b.Id == bookId);
                if (book != null) _userBooks[userId].Remove(book);
            }
        }

        /// <summary>
        /// Removes all books from the in-memory collection for the given user.
        /// </summary>
        /// <param name="userId">The id of the user to remove all books for.</param>
        public void RemoveAllForUser(Guid userId)
        {
            _userBooks.Remove(userId);
        }

        /// <summary>
        /// Updates the book in the in-memory collection with the given id and user id with the new book data.
        /// </summary>
        /// <param name="userId">The id of the user to update the book for.</param>
        /// <param name="book">The book to update in the collection.</param>
        public void Update(Guid userId, Book book)
        {
            if (_userBooks.TryGetValue(userId, out List<Book>? value))
            {
                var idx = value.FindIndex(b => b.Id == book.Id);
                if (idx >= 0) _userBooks[userId][idx] = book;
            }
        }
    }
}