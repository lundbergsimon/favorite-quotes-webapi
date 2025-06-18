namespace FavoriteQuoutesWebApi.Models;

public class Book
{
    public String Title { get; set; } = string.Empty;
    public String Author { get; set; } = string.Empty;
    public DateTime? DatePublished { get; set; } = null;
}