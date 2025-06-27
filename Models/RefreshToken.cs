namespace FavoriteQuoutesWebApi.Models
{
    public class RefreshToken
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; } = string.Empty;
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; } = string.Empty;
        public string? ReplacedByToken { get; set; } = string.Empty;
        public bool IsActive => Revoked == null && Expires >= DateTime.UtcNow;
    }
}
