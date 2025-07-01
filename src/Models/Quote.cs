namespace FavoriteQuoutesWebApi.Models;

public class Quote
{
    public int Id { get; set; }
    public String Text { get; set; } = string.Empty;
    public int BookId { get; set; }
}
