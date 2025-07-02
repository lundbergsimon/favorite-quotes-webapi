namespace FavoriteQuoutesWebApi.Storage
{
    using FavoriteQuoutesWebApi.Models;

    public interface IRefreshTokenStore
    {
        IEnumerable<RefreshToken> GetByUserId(Guid userId);
        RefreshToken? GetByToken(string token);
        void Add(RefreshToken token);
        void RemoveAllForUser(Guid userId);
        void RemoveByToken(string token);
        void Update(RefreshToken token);
    }
}