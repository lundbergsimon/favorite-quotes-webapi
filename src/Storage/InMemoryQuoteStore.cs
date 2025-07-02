
namespace FavoriteQuoutesWebApi.Storage
{
    using FavoriteQuoutesWebApi.Models;

    public class InMemoryQuoteStore : IQuoteStore
    {
        private readonly Dictionary<Guid, List<Quote>> _userQuotes = [];

        public IEnumerable<Quote> GetByUserId(Guid userId) => _userQuotes.TryGetValue(userId, out List<Quote>? value) ? value : Enumerable.Empty<Quote>();
        public Quote? GetById(Guid userId, int quoteId) => _userQuotes.TryGetValue(userId, out List<Quote>? value) ? value.FirstOrDefault(q => q.Id == quoteId) : null;

        /// <summary>
        /// Adds a quote to the in-memory collection for the given user.
        /// </summary>
        /// <param name="userId">The id of the user to add the quote for.</param>
        /// <param name="quote">The quote to add to the collection.</param>
        public void Add(Guid userId, Quote quote)
        {
            if (!_userQuotes.ContainsKey(userId)) _userQuotes[userId] = [];
            _userQuotes[userId].Add(quote);
        }

        /// <summary>
        /// Removes a quote from the in-memory collection with the given id and user id.
        /// </summary>
        /// <param name="userId">The id of the user to remove the quote for.</param>
        /// <param name="quoteId">The id of the quote to remove.</param>
        public void Remove(Guid userId, int quoteId)
        {
            if (_userQuotes.TryGetValue(userId, out List<Quote>? value))
            {
                var quote = value.FirstOrDefault(q => q.Id == quoteId);
                if (quote != null) _userQuotes[userId].Remove(quote);
            }
        }

        /// <summary>
        /// Removes all quotes from the in-memory collection for the given user.
        /// </summary>
        /// <param name="userId">The id of the user to remove all quotes for.</param>
        public void RemoveAllForUser(Guid userId)
        {
            _userQuotes.Remove(userId);
        }

        /// <summary>
        /// Updates the quote in the in-memory collection with the given id and user id with the new quote data.
        /// </summary>
        /// <param name="userId">The id of the user to update the quote for.</param>
        /// <param name="quote">The quote to update in the collection.</param>
        public void Update(Guid userId, Quote quote)
        {
            if (_userQuotes.TryGetValue(userId, out List<Quote>? value))
            {
                var idx = value.FindIndex(q => q.Id == quote.Id);
                if (idx >= 0) _userQuotes[userId][idx] = quote;
            }
        }
    }

}