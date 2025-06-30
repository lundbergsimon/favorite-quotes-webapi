namespace FavoriteQuoutesWebApi.Models;

public class QuoteCreateRequest
{
    public String Text { get; set; } = string.Empty;
    public int BookId { get; set; }
}
