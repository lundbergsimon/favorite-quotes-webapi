namespace FavoriteQuoutesWebApi.Models
{
    public class AuthResponse
    {
        public string AccessToken { get; set; } = null!;
        public UserResponse User { get; set; } = null!;
    }
}