
namespace FavoriteQuoutesWebApi.Storage
{
    using FavoriteQuoutesWebApi.Models;

    public class InMemoryUserStore : IUserStore
    {
        private readonly List<User> _users = [];

        /// <summary>
        /// Retrieves a user from the in-memory collection by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to retrieve.</param>
        /// <returns>The user if found, otherwise null.</returns>
        public User? GetById(Guid id)
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }

        /// <summary>
        /// Retrieves a user from the in-memory collection by their username.
        /// </summary>
        /// <param name="username">The username of the user to retrieve.</param>
        /// <returns>The user if found, otherwise null.</returns>
        public User? GetByUsername(string username)
        {
            return _users.FirstOrDefault(u => u.Username == username);
        }

        /// <summary>
        /// Retrieves a collection of all users from the in-memory collection.
        /// </summary>
        /// <returns>A collection of all users.</returns>
        public IEnumerable<User> GetAll()
        {
            return _users;
        }

        /// <summary>
        /// Adds a user to the in-memory collection.
        /// </summary>
        /// <param name="user">The user to add to the collection.</param>
        public void Add(User user)
        {
            _users.Add(user);
        }

        /// <summary>
        /// Removes a user from the in-memory collection by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user to remove.</param>
        public void Remove(Guid id)
        {
            var user = GetById(id);
            if (user != null) _users.Remove(user);
        }
    }
}