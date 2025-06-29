namespace FavoriteQuoutesWebApi.Controllers
{
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using FavoriteQuoutesWebApi.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.IdentityModel.Tokens;

    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        // TODO: Replace with a database
        private static List<User> users = new List<User>();
        private static List<RefreshToken> refreshTokens = new List<RefreshToken>();

        private readonly IConfiguration _configuration;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpirationMinutes;
        private readonly int _refreshTokenExpirationDays;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            var jwtSettings = _configuration.GetSection("Jwt");
            _secretKey = jwtSettings["Key"]!;
            _issuer = jwtSettings["Issuer"]!;
            _audience = jwtSettings["Audience"]!;
            _accessTokenExpirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"]!);
            _refreshTokenExpirationDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"]!);
        }

        private string GenerateAccessToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]{
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
            };

            var token = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenExpirationMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshToken GenerateRefreshtoken(User user, string ipAddress)
        {
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = Guid.NewGuid().ToString("N"),
                Expires = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };
            return refreshToken;
        }

        private void SetRefreshTokenCookie(RefreshToken refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = new DateTimeOffset(refreshToken.Expires),
                SameSite = SameSiteMode.None,
                Secure = true
            };
            Response.Cookies.Append("refreshToken", refreshToken.Token, cookieOptions);
        }

        private void ClearRefreshTokenCookie()
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(-1), // Set expiry to past to ensure deletion
                SameSite = SameSiteMode.None,
                Secure = true // Must match SetRefreshTokenCookie's Secure setting
            };
            Response.Cookies.Delete("refreshToken", cookieOptions);
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (users.Any(u => u.Username == request.Username))
                return Conflict(new { message = "Username already exists." });

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = request.Password // TODO: Hash password
            };

            // TODO: Replace with a database
            users.Add(newUser);

            var accessToken = GenerateAccessToken(newUser);
            var refreshToken = GenerateRefreshtoken(newUser, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            // TODO: Replace with a database
            refreshTokens.Add(refreshToken);

            // Set refresh token as HTTP-only cookie
            SetRefreshTokenCookie(refreshToken);

            Console.WriteLine($"Refresh tokens (reg): {System.Text.Json.JsonSerializer.Serialize(refreshTokens)}");

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                User = new UserResponse { Id = newUser.Id, Username = newUser.Username }
            });
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = users.FirstOrDefault(u => u.Username == request.Username);

            // TODO: Validate password
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials." });

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshtoken(user, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            // TODO: Revoke any existing refresh tokens for this user for security
            var existingRefreshTokens = refreshTokens.Where(t => t.UserId == user.Id && t.IsActive).ToList();
            foreach (var token in existingRefreshTokens)
            {
                token.Revoked = DateTime.UtcNow;
                token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            // TODO: Replace with a database
            refreshTokens.Add(refreshToken);

            // Set refresh token as HTTP-only cookie
            SetRefreshTokenCookie(refreshToken);

            Console.WriteLine($"Refresh tokens (li): {System.Text.Json.JsonSerializer.Serialize(refreshTokens)}");

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                User = new UserResponse { Id = user.Id, Username = user.Username }
            });
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            Console.WriteLine($"Refresh tokens (rt): {System.Text.Json.JsonSerializer.Serialize(refreshTokens)}");

            var refreshTokenString = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshTokenString))
            {
                return Unauthorized(new { message = "Refresh token not found." });
            }

            var refreshToken = refreshTokens.SingleOrDefault(t => t.Token == refreshTokenString && t.IsActive);
            if (refreshToken == null)
            {
                // Refresh token is invalid or revoked. Attempt to detect reuse.
                var userWhoTriedToReuse = refreshTokens.SingleOrDefault(t => t.Token == refreshTokenString)?.UserId;
                if (userWhoTriedToReuse != null)
                {
                    // If a revoked token was sent, invalidate all tokens for that user
                    var allUserTokens = refreshTokens.Where(t => t.UserId == userWhoTriedToReuse).ToList();
                    foreach (var token in allUserTokens)
                    {
                        token.Revoked = DateTime.UtcNow;
                        token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                    }
                    Console.WriteLine($"Token reuse detected for User ID {userWhoTriedToReuse}. All tokens revoked.");
                }
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            var user = users.SingleOrDefault(u => u.Id == refreshToken.UserId);
            if (user == null)
            {
                return Unauthorized(new { message = "User associated with refresh token not found." });
            }

            // Generate new access and refresh tokens
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshtoken(user, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            // Mark old refreshToken as replaced
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            // TODO: Replace with a database
            refreshTokens.Add(newRefreshToken);

            // Set refresh token as HTTP-only cookie
            SetRefreshTokenCookie(newRefreshToken);

            return Ok(new AuthResponse
            {
                AccessToken = newAccessToken,
                User = new UserResponse { Id = user.Id, Username = user.Username }
            });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var refreshTokenString = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshTokenString))
            {
                var refreshToken = refreshTokens.SingleOrDefault(t => t.Token == refreshTokenString);
                if (refreshToken != null)
                {
                    refreshToken.Revoked = DateTime.UtcNow;
                    refreshToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                    Console.WriteLine($"Refresh token for user {refreshToken.UserId} revoked upon logout.");
                }
            }

            ClearRefreshTokenCookie();
            return Ok(new { message = "Logged out successfully!" });
        }
    }
}