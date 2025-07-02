
namespace FavoriteQuoutesWebApi.Storage
{
    using FavoriteQuoutesWebApi.Models;

    public class InMemoryRefreshTokenStore : IRefreshTokenStore
    {
        private readonly List<RefreshToken> _tokens = [];

        /// <summary>
        /// Retrieves a collection of refresh tokens associated with the given user id from the store.
        /// </summary>
        /// <param name="userId">The id of the user whose refresh tokens are to be retrieved.</param>
        /// <returns>A collection of refresh tokens associated with the given user id.</returns>
        public IEnumerable<RefreshToken> GetByUserId(Guid userId)
        {
            return _tokens.Where(t => t.UserId == userId);
        }

        /// <summary>
        /// Retrieves a refresh token by its token string from the store.
        /// </summary>
        /// <param name="token">The token string of the refresh token to retrieve.</param>
        /// <returns>The refresh token if found, otherwise null.</returns>
        public RefreshToken? GetByToken(string token)
        {
            return _tokens.FirstOrDefault(t => t.Token == token);
        }

        /// <summary>
        /// Adds the refresh token to the store with the given token.
        /// </summary>
        /// <param name="token">The token of the refresh token to add to the store.</param>
        public void Add(RefreshToken token) => _tokens.Add(token);

        /// <summary>
        /// Removes all refresh tokens associated with the specified user id from the store.
        /// </summary>
        /// <param name="userId">The id of the user whose refresh tokens are to be removed.</param>
        public void RemoveAllForUser(Guid userId) => _tokens.RemoveAll(t => t.UserId == userId);

        /// <summary>
        /// Removes the refresh token from the store with the given token.
        /// </summary>
        /// <param name="token">The token of the refresh token to remove.</param>
        public void RemoveByToken(string token)
        {
            var t = GetByToken(token);
            if (t != null) _tokens.Remove(t);
        }

        /// <summary>
        /// Updates the refresh token with the given token in the store.
        /// </summary>
        /// <param name="token">The refresh token to update in the store.</param>
        public void Update(RefreshToken token)
        {
            var idx = _tokens.FindIndex(t => t.Token == token.Token);
            if (idx >= 0) _tokens[idx] = token;
        }
    }
}