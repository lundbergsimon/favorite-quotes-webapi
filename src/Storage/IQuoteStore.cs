namespace FavoriteQuoutesWebApi.Storage
{
    using FavoriteQuoutesWebApi.Models;

    public interface IQuoteStore
    {
        IEnumerable<Quote> GetByUserId(Guid userId);
        Quote? GetById(Guid userId, int quoteId);
        void Add(Guid userId, Quote quote);
        void Remove(Guid userId, int quoteId);
        void RemoveAllForUser(Guid userId);
        void Update(Guid userId, Quote quote);
    }
}