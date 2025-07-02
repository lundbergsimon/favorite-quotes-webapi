namespace FavoriteQuoutesWebApi.Storage
{
    using FavoriteQuoutesWebApi.Models;

    public interface IBookStore
    {
        IEnumerable<Book> GetByUserId(Guid userId);
        Book? GetById(Guid userId, int bookId);
        void Add(Guid userId, Book book);
        void Remove(Guid userId, int bookId);
        void RemoveAllForUser(Guid userId);
        void Update(Guid userId, Book book);
    }
}