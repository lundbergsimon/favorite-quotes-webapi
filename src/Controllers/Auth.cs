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
        private readonly IUserStore _userStore;
        private readonly IRefreshTokenStore _refreshTokenStore;
        private readonly IBookStore _bookStore;
        private readonly IQuoteStore _quoteStore;

        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _accessTokenExpirationMinutes;
        private readonly int _refreshTokenExpirationDays;
        private readonly CookieOptions _cookieOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthController"/> class.
        /// Sets up JWT configuration, secret key, issuer, audience, and token expiration times from configuration settings.
        /// Configures cookie options based on the environment.
        /// </summary>
        /// <param name="configuration">The configuration interface providing access to application settings.</param>
        /// <param name="env">The hosting environment interface providing environment-specific information.</param>
        public AuthController(
            IConfiguration configuration,
            IWebHostEnvironment env,
            IUserStore userStore,
            IRefreshTokenStore refreshTokenStore,
            IBookStore bookStore,
            IQuoteStore quoteStore)
        {
            _env = env;
            _configuration = configuration;
            _userStore = userStore;
            _refreshTokenStore = refreshTokenStore;
            _bookStore = bookStore;
            _quoteStore = quoteStore;
            var jwtSettings = _configuration.GetSection("Jwt");
            _secretKey = jwtSettings["Key"]!;
            _issuer = jwtSettings["Issuer"]!;
            _audience = jwtSettings["Audience"]!;
            _accessTokenExpirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"]!);
            _refreshTokenExpirationDays = int.Parse(jwtSettings["RefreshTokenExpirationDays"]!);
            _cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                SameSite = _env.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
                Secure = !_env.IsDevelopment()
            };
        }

        /// <summary>
        /// Generates a JWT access token for the given user.
        /// </summary>
        /// <param name="user">The user to generate the token for.</param>
        /// <returns>A JWT access token string.</returns>
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

        /// <summary>
        /// Generates a refresh token for the given user and IP address.
        /// </summary>
        /// <param name="user">The user to generate the token for.</param>
        /// <param name="ipAddress">The IP address of the client device.</param>
        /// <returns>A new <see cref="RefreshToken"/> instance.</returns>
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
            _cookieOptions.Expires = new DateTimeOffset(refreshToken.Expires);
            Response.Cookies.Append("refreshToken", refreshToken.Token, _cookieOptions);
        }

        private void ClearRefreshTokenCookie()
        {
            _cookieOptions.Expires = DateTime.UtcNow.AddDays(-1);
            Response.Cookies.Delete("refreshToken", _cookieOptions);
        }

        /// <summary>
        /// Registers a new user account.
        /// </summary>
        /// <param name="request">The registration request containing the username and password.</param>
        /// <returns>An authentication response containing an access token and user information.</returns>
        /// <remarks>If the username already exists, a conflict status code is returned.</remarks>
        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest request)
        {
            if (_userStore.GetByUsername(request.Username) != null)
                return Conflict(new { message = "Username already exists." });

            var newUser = new User
            {
                Username = request.Username,
                PasswordHash = request.Password // TODO: Hash password
            };

            _userStore.Add(newUser);

            var accessToken = GenerateAccessToken(newUser);
            var refreshToken = GenerateRefreshtoken(newUser, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            _refreshTokenStore.Add(refreshToken);

            // Set refresh token as HTTP-only cookie
            SetRefreshTokenCookie(refreshToken);

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                User = new UserResponse { Id = newUser.Id, Username = newUser.Username }
            });
        }

        /// <summary>
        /// Logs in a user account.
        /// </summary>
        /// <param name="request">The login request containing the username and password.</param>
        /// <returns>An authentication response containing an access token and user information.</returns>
        /// <remarks>If the username or password is invalid, an unauthorized status code is returned.</remarks>
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            var user = _userStore.GetByUsername(request.Username);

            if (user == null || user.PasswordHash != request.Password) // TODO: Hash password
                return Unauthorized(new { message = "Invalid credentials." });

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshtoken(user, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            // Revoke any existing refresh tokens for this user for security
            var existingRefreshTokens = _refreshTokenStore.GetByUserId(user.Id);
            foreach (var token in existingRefreshTokens)
            {
                token.Revoked = DateTime.UtcNow;
                token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            _refreshTokenStore.Add(refreshToken);

            SetRefreshTokenCookie(refreshToken);

            return Ok(new AuthResponse
            {
                AccessToken = accessToken,
                User = new UserResponse { Id = user.Id, Username = user.Username }
            });
        }

        /// <summary>
        /// Refreshes an access token given a valid refresh token.
        /// </summary>
        /// <returns>An authentication response containing an access token and user information.</returns>
        /// <remarks>
        /// If the refresh token is invalid, expired, or revoked, an unauthorized status code is returned.
        /// If the refresh token has been reused, all tokens for that user are revoked.
        /// </remarks>
        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            var refreshTokenString = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshTokenString))
            {
                return Unauthorized(new { message = "Refresh token not found." });
            }

            var refreshToken = _refreshTokenStore.GetByToken(refreshTokenString);
            if (refreshToken == null)
            {
                // Refresh token is invalid or revoked. Attempt to detect reuse.
                var userWhoTriedToReuse = _refreshTokenStore.GetByToken(refreshTokenString)?.UserId;
                if (userWhoTriedToReuse != null)
                {
                    // If a revoked token was sent, invalidate all tokens for that user
                    var allUserTokens = _refreshTokenStore.GetByUserId(userWhoTriedToReuse.Value);
                    foreach (var token in allUserTokens)
                    {
                        token.Revoked = DateTime.UtcNow;
                        token.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                    }
                }
                return Unauthorized(new { message = "Invalid or expired refresh token." });
            }

            var user = _userStore.GetById(refreshToken.UserId);
            if (user == null)
            {
                return Unauthorized(new { message = "User associated with refresh token not found." });
            }

            // Generate new access and refresh tokens
            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshtoken(user, HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown");

            // Mark old refreshToken as revoked and replaced
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
            refreshToken.ReplacedByToken = newRefreshToken.Token;

            _refreshTokenStore.Update(refreshToken);
            _refreshTokenStore.Add(newRefreshToken);

            // Set refresh token as HTTP-only cookie
            SetRefreshTokenCookie(newRefreshToken);

            return Ok(new AuthResponse
            {
                AccessToken = newAccessToken,
                User = new UserResponse { Id = user.Id, Username = user.Username }
            });
        }

        /// <summary>
        /// Revokes the refresh token associated with the user that made this request,
        /// and removes the refresh token cookie from the client.
        /// </summary>
        /// <returns>A JSON response with a success message.</returns>
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            var refreshTokenString = Request.Cookies["refreshToken"];
            if (!string.IsNullOrEmpty(refreshTokenString))
            {
                // Revoke the refresh token
                var refreshToken = _refreshTokenStore.GetByToken(refreshTokenString);
                if (refreshToken != null)
                {
                    refreshToken.Revoked = DateTime.UtcNow;
                    refreshToken.RevokedByIp = HttpContext.Connection.RemoteIpAddress?.ToString();
                }
            }

            // Clear refresh token cookie on the client
            ClearRefreshTokenCookie();

            // Return success response
            return Ok(new { message = "Logged out successfully!" });
        }
    }
}