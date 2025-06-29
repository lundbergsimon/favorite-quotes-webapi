namespace FavoriteQuoutesWebApi.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public String Username { get; set; } = string.Empty;
    public String PasswordHash { get; set; } = string.Empty;

}