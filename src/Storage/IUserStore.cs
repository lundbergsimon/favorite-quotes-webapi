namespace FavoriteQuoutesWebApi.Storage
{
    using FavoriteQuoutesWebApi.Models;

    public interface IUserStore
    {
        User? GetById(Guid id);
        User? GetByUsername(string username);
        IEnumerable<User> GetAll();
        void Add(User user);
        void Remove(Guid id);
    }
}